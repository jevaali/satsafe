using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.Entities.Identity;
using App.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App.Tests.Integration.Helpers;

public static class DataSeeder
{
    public static void SeedData(IServiceProvider rootServices)
    {
        SeedAsync(rootServices).GetAwaiter().GetResult();
    }

    public static async Task SeedAsync(IServiceProvider rootServices)
    {
        using var scope = rootServices.CreateScope();
        var sp = scope.ServiceProvider;

        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<AppRole>>();
        var ctx = sp.GetRequiredService<AppDbContext>();

        await EnsureUserAsync(userManager, TestUsers.UserAId, TestUsers.UserAEmail, TestUsers.Password);
        await EnsureUserAsync(userManager, TestUsers.UserBId, TestUsers.UserBEmail, TestUsers.Password);

        await SeedItemsAsync(ctx, userManager, roleManager);
    }

    public static async Task SeedItemsAsync(
        AppDbContext ctx,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        // Roles
        foreach (var role in new[] { "User", "Operator", "OperatorHelper" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new AppRole { Name = role });
        }

        var userA = await userManager.FindByIdAsync(TestUsers.UserAId.ToString());
        if (userA != null && !await userManager.IsInRoleAsync(userA, "Operator"))
            await userManager.AddToRoleAsync(userA, "Operator");

        var userB = await userManager.FindByIdAsync(TestUsers.UserBId.ToString());
        if (userB != null && !await userManager.IsInRoleAsync(userB, "OperatorHelper"))
            await userManager.AddToRoleAsync(userB, "OperatorHelper");

        // Operator
        if (!ctx.Operators.Any(o => o.Id == TestUsers.OperatorId))
        {
            ctx.Operators.Add(new Operator
            {
                Id = TestUsers.OperatorId,
                Name = "Test Operator",
                CountryCode = "EE",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder"
            });
            await ctx.SaveChangesAsync();
        }

        // UserOperator links
        if (!ctx.UserOperators.Any(uo => uo.AppUserId == TestUsers.UserAId))
            ctx.UserOperators.Add(new UserOperator { AppUserId = TestUsers.UserAId, OperatorId = TestUsers.OperatorId });
        if (!ctx.UserOperators.Any(uo => uo.AppUserId == TestUsers.UserBId))
            ctx.UserOperators.Add(new UserOperator { AppUserId = TestUsers.UserBId, OperatorId = TestUsers.OperatorId });

        await ctx.SaveChangesAsync();

        // Satellite
        if (!ctx.Satellites.Any(s => s.Id == TestUsers.TestSatelliteId))
        {
            var op = ctx.Operators.First(o => o.Id == TestUsers.OperatorId);
            var sat = Satellite.Create(TestUsers.TestSatelliteName, DateTime.UtcNow.AddYears(-1), op);
            sat.Id = TestUsers.TestSatelliteId;
            sat.NoradId = TestUsers.TestSatelliteNoradId;
            sat.Operator = null; // op is already tracked; clear nav to avoid conflict
            ctx.Satellites.Add(sat);
            await ctx.SaveChangesAsync();
        }

        // ConjunctionEvent + Alert for UserA
        if (!ctx.Alerts.Any(a => a.Id == TestUsers.TestAlertId))
        {
            var sat = ctx.Satellites.First(s => s.Id == TestUsers.TestSatelliteId);

            var debris = Satellite.CreateFromExternalData("debris-for-test", DateTime.UtcNow.AddYears(-2), null, null);
            ctx.Satellites.Add(debris);
            await ctx.SaveChangesAsync();

            var conj = new ConjunctionEvent(sat.Id, debris.Id, DateTime.UtcNow.AddHours(6), 2000, 5000)
            {
                Id = TestUsers.TestConjunctionId
            };
            ctx.ConjunctionEvents.Add(conj);
            await ctx.SaveChangesAsync();

            ctx.Alerts.Add(new Alert
            {
                Id = TestUsers.TestAlertId,
                ConjunctionEventId = conj.Id,
                UserId = TestUsers.UserAId,
                Severity = EAlertSeverity.High,
                IsAcknowledged = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "seeder"
            });
            await ctx.SaveChangesAsync();
        }
    }

    private static async Task EnsureUserAsync(UserManager<AppUser> userManager, Guid id, string email, string password)
    {
        if (await userManager.FindByEmailAsync(email) != null) return;

        var user = new AppUser
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var res = await userManager.CreateAsync(user, password);
        if (!res.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create test user {email}: " +
                string.Join(", ", res.Errors.Select(e => e.Description)));
    }
}
