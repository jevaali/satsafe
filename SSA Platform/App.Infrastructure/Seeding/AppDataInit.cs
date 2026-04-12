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
            CountryCode  = "EE",
            OperatorType = EOperatorType.Commercial,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = "seed"
        };
        context.Operators.Add(op);
        context.SaveChanges();

        // 3. Link user → operator
        context.UserOperators.Add(new UserOperator
        {
            Id         = Guid.NewGuid(),
            AppUserId  = user.Id,
            OperatorId = op.Id
        });
        context.SaveChanges();

        // 4. Satellites
        // A & B — nearly identical orbit; will produce a conjunction when screening runs
        var satA = Satellite.Create("SENTINEL-A", DateTime.UtcNow.AddYears(-2), op);
        satA.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_778_000,
            eccentricity:      0.0001,
            inclination:       51.6,
            RAAN:              95.0,
            argumentOfPerigee: 120.0,
            meanAnomaly:       45.0
        ));

        var satB = Satellite.Create("SENTINEL-B", DateTime.UtcNow.AddYears(-2), op);
        satB.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_778_100,
            eccentricity:      0.0001,
            inclination:       51.6,
            RAAN:              95.0,
            argumentOfPerigee: 120.0,
            meanAnomaly:       45.5
        ));

        // C — same orbit plane as A, slight RAAN offset; crosses A and B periodically
        var satC = Satellite.Create("SENTINEL-C", DateTime.UtcNow.AddYears(-1), op);
        satC.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_778_050,
            eccentricity:      0.0001,
            inclination:       51.6,
            RAAN:              95.0,
            argumentOfPerigee: 120.0,
            meanAnomaly:       46.2
        ));

        // D — slightly higher orbit, different inclination; orbit plane crosses A/B/C
        var satD = Satellite.Create("GUARDIAN-1", DateTime.UtcNow.AddYears(-1), op);
        satD.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_788_000,
            eccentricity:      0.0003,
            inclination:       52.1,
            RAAN:              94.0,
            argumentOfPerigee: 100.0,
            meanAnomaly:       20.0
        ));

        // E — polar-ish orbit; crosses other orbital planes twice per orbit
        var satE = Satellite.Create("GUARDIAN-2", DateTime.UtcNow.AddMonths(-6), op);
        satE.SetOrbit(new OrbitParameters(
            semiMajorAxis:     6_780_000,
            eccentricity:      0.0002,
            inclination:       97.4,
            RAAN:              170.0,
            argumentOfPerigee: 90.0,
            meanAnomaly:       0.0
        ));

        context.Satellites.AddRange(satA, satB, satC, satD, satE);
        context.SaveChanges();

        // 5. Pre-seed two conjunction events with future TCAs so the UI is testable
        //    immediately — no need to trigger screening manually first.
        const double threshold = 5_000; // 5 km

        // High-risk event: A vs C, TCA ~14 h from now, miss ~650 m
        var evtHighRisk = new ConjunctionEvent(
            satA.Id, satC.Id,
            DateTime.UtcNow.AddHours(14),
            missDistance: 650,
            threshold:    threshold);
        evtHighRisk.UpdateCollisionProbability(0.035);
        evtHighRisk.RefreshStatus();

        // Normal-risk event: B vs D, TCA ~38 h from now, miss ~3 200 m
        var evtNormal = new ConjunctionEvent(
            satB.Id, satD.Id,
            DateTime.UtcNow.AddHours(38),
            missDistance: 3_200,
            threshold:    threshold);
        evtNormal.UpdateCollisionProbability(0.002);
        evtNormal.RefreshStatus();

        context.ConjunctionEvents.AddRange(evtHighRisk, evtNormal);
        context.SaveChanges();

        // Alerts for both events
        var alertHigh = evtHighRisk.CreateAlert(user.Id, alertThresholdKm: 5.0);
        var alertNorm = evtNormal.CreateAlert(user.Id, alertThresholdKm: 5.0);
        if (alertHigh != null) context.Alerts.Add(alertHigh);
        if (alertNorm != null) context.Alerts.Add(alertNorm);
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
