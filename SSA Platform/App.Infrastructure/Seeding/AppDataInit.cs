using App.Domain.Entities;
using App.Domain.Entities.Identity;
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

    public static void SeedAppData(AppDbContext context)
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
