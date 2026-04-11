using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.Entities.Identity;
using App.Domain.ValueObjects;
using App.Infrastructure.Persistence;
using App.Infrastructure.Seeding.dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Seeding;

public static class AppDataInit
{
    public static void DeleteDatabase(AppDbContext context)
    {
        context.Database.EnsureDeleted();
    }

    public static void MigrateDatabase(AppDbContext context)
    {
        context.Database.Migrate();
    }

    public static void SeedAppData(AppDbContext context, UserManager<AppUser> userManager)
    {
        Console.WriteLine("Seeding database...");
        JsonSeeder.Seed<Satellite, SatelliteSeedDto>(
            context,
            "satellites.json",
            dto => Satellite.CreateFromExternalData(
                dto.Name,
                dto.LaunchDate,
                dto.OperatorName,
                dto.OrbitParameters
            ),
            context.Satellites
        );
        
        Console.WriteLine("Seeding database...");

        JsonSeeder.Seed<DebrisObject, DebrisSeedDto>(
            context,
            "debris.json",
            dto => DebrisObject.CreateFromExternalData(
                dto.NoradId,
                dto.SizeEstimate,
                dto.RiskCategory,
                dto.OrbitParameters
            ),
            context.DebrisObjects
        );
        
        SeedConjunctionTestData(context, userManager);
    }
    
    private static void SeedConjunctionTestData(AppDbContext context, UserManager<AppUser> userManager)
    {
        const string operatorEmail = "operator@ssa.com";
        if (context.Satellites.Any(s => s.OperatorName == "SSA-TEST-OPS")) return;

        // 1. User first
        var user = userManager.FindByEmailAsync(operatorEmail).Result;
        if (user == null)
        {
            user = new AppUser
            {
                Email          = operatorEmail,
                UserName       = operatorEmail,
                FirstName      = "Test",
                LastName       = "Operator",
                EmailConfirmed = true,
                CreatedAt      = DateTime.UtcNow,
                UpdatedAt      = DateTime.UtcNow
            };
            var result = userManager.CreateAsync(user, "Operator@123").Result;
            if (!result.Succeeded)
                throw new ApplicationException("Operator user seeding failed: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));

            userManager.AddToRoleAsync(user, "Operator").Wait();
        }

        // 2. Operator — save before linking
        var op = new Operator
        {
            Id           = Guid.NewGuid(),
            Name         = "SSA-TEST-OPS",
            CountryCode      = "EE",
            OperatorType = EOperatorType.Commercial,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = "seed"
        };
        context.Operators.Add(op);
        context.SaveChanges(); // ✅ operator must exist before UserOperator references it

        // 3. Link user → operator
        context.UserOperators.Add(new UserOperator
        {
            Id         = Guid.NewGuid(),
            AppUserId  = user.Id,
            OperatorId = op.Id
        });
        context.SaveChanges(); // ✅ commit the link before adding satellites

        // 4. Satellites — operator already in DB so FK is valid
        var satA = Satellite.Create("SENTINEL-A", DateTime.UtcNow.AddYears(-1), op);
        satA.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_778_000,
            eccentricity:      0.0001,
            inclination:       51.6,
            RAAN:              95.0,
            argumentOfPerigee: 120.0,
            meanAnomaly:       45.0
        ));

        var satB = Satellite.Create("SENTINEL-B", DateTime.UtcNow.AddYears(-1), op);
        satB.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_778_100,
            eccentricity:      0.0001,
            inclination:       51.6,
            RAAN:              95.0,
            argumentOfPerigee: 120.0,
            meanAnomaly:       45.5
        ));

        context.Satellites.Add(satA);
        context.Satellites.Add(satB);
        context.SaveChanges();

        Console.WriteLine("✅ Conjunction test data seeded — login: operator@ssa.com / Operator@123");
    }
        
     public static void SeedIdentity(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        foreach (var roleName in InitialData.Roles)
        {
            var role = roleManager.FindByNameAsync(roleName).Result;

            if (role != null) continue;

            role = new AppRole()
            {
                Name = roleName,
            };

            var result = roleManager.CreateAsync(role).Result;
            if (!result.Succeeded)
            {
                throw new ApplicationException("Role creation failed!");
            }
        }


        foreach (var userInfo in InitialData.Users)
        {
            var user = userManager.FindByEmailAsync(userInfo.email).Result;
            if (user == null)
            {
                user = new AppUser()
                {
                    Email = userInfo.email,
                    UserName = userInfo.email,
                    EmailConfirmed = true
                };
                var result = userManager.CreateAsync(user, userInfo.password).Result;
                if (!result.Succeeded)
                {
                    throw new ApplicationException("User creation failed!");
                }
            }

            foreach (var role in userInfo.roles)
            {
                if (userManager.IsInRoleAsync(user, role).Result)
                {
                    Console.WriteLine($"User {user.UserName} already in role {role}");
                    continue;
                }
                
                var roleResult = userManager.AddToRoleAsync(user, role).Result;
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }
                else
                {
                    Console.WriteLine($"User {user.UserName} added to role {role}");
                }
            }
        }
    }

}
