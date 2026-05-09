using App.Tests.Integration.Helpers;

namespace App.Tests.Integration.DAL;

using Application.Mappers;
using Domain.Entities;
using Domain.Entities.Enums;
using Infrastructure.Repositories;
using AwesomeAssertions;

public class AlertRepository_Tests : RepositoryTestBase
{
    private async Task<(Satellite sat1, Satellite sat2, ConjunctionEvent conj)> SetupConjunctionAsync()
    {
        await using var ctx = CreateContext();

        var sat1 = Satellite.CreateFromExternalData("Sat-1", DateTime.UtcNow, null, null);
        var sat2 = Satellite.CreateFromExternalData("Sat-2", DateTime.UtcNow, null, null);
        ctx.Satellites.AddRange(sat1, sat2);
        await ctx.SaveChangesAsync();

        var conj = new ConjunctionEvent(sat1.Id, sat2.Id, DateTime.UtcNow.AddHours(12), 3000, 5000);
        ctx.ConjunctionEvents.Add(conj);
        await ctx.SaveChangesAsync();

        return (sat1, sat2, conj);
    }

    private async Task<Alert> AddAlertAsync(Guid conjunctionId, Guid userId)
    {
        await using var ctx = CreateContext();
        var alert = new Alert
        {
            ConjunctionEventId = conjunctionId,
            UserId = userId,
            Severity = EAlertSeverity.High,
            IsAcknowledged = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
        ctx.Alerts.Add(alert);
        await ctx.SaveChangesAsync();
        return alert;
    }

    [Fact]
    public async Task GetByConjunctionIdsAsync_ReturnsMatchingAlerts()
    {
        var (_, _, conj) = await SetupConjunctionAsync();
        await AddAlertAsync(conj.Id, TestUsers.UserAId);
        await AddAlertAsync(conj.Id, TestUsers.UserBId);

        await using var ctx = CreateContext();
        var repo = new AlertRepository(ctx, new IdentityMapper<Alert>());

        var results = (await repo.GetByConjunctionIdsAsync([conj.Id])).ToList();

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(a => a.ConjunctionEventId.Should().Be(conj.Id));
    }

    [Fact]
    public async Task GetByConjunctionIdsAsync_ExcludesAlertsForOtherConjunctions()
    {
        var (sat1, sat2, conjA) = await SetupConjunctionAsync();

        await using var setupCtx = CreateContext();
        var conjB = new ConjunctionEvent(sat1.Id, sat2.Id, DateTime.UtcNow.AddHours(24), 4000, 5000);
        setupCtx.ConjunctionEvents.Add(conjB);
        await setupCtx.SaveChangesAsync();

        await AddAlertAsync(conjA.Id, TestUsers.UserAId);
        await AddAlertAsync(conjB.Id, TestUsers.UserAId);

        await using var ctx = CreateContext();
        var repo = new AlertRepository(ctx, new IdentityMapper<Alert>());

        var results = (await repo.GetByConjunctionIdsAsync([conjA.Id])).ToList();

        results.Should().HaveCount(1);
        results[0].ConjunctionEventId.Should().Be(conjA.Id);
    }

    [Fact]
    public async Task GetByConjunctionIdsAsync_EmptyList_ReturnsNoAlerts()
    {
        var (_, _, conj) = await SetupConjunctionAsync();
        await AddAlertAsync(conj.Id, TestUsers.UserAId);

        await using var ctx = CreateContext();
        var repo = new AlertRepository(ctx, new IdentityMapper<Alert>());

        var results = (await repo.GetByConjunctionIdsAsync([])).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task AllAsync_ReturnsAllAlerts()
    {
        var (_, _, conj) = await SetupConjunctionAsync();
        await AddAlertAsync(conj.Id, TestUsers.UserAId);
        await AddAlertAsync(conj.Id, TestUsers.UserBId);

        await using var ctx = CreateContext();
        var repo = new AlertRepository(ctx, new IdentityMapper<Alert>());

        var all = (await repo.AllAsync()).ToList();

        all.Should().HaveCount(2);
    }
}
