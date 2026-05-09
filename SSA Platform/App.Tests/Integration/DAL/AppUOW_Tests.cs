using App.Tests.Integration.Helpers;

namespace App.Tests.Integration.DAL;

using Application.Mappers;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Repositories;
using Infrastructure.UnitOfWork;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

[Collection("CultureSensitive")]
public class AppUOW_Tests : RepositoryTestBase
{
    private UnitOfWork CreateUow()
    {
        var ctx = CreateContext();
        var im = new IdentityMapper<Satellite>();
        var dm = new IdentityMapper<DebrisObject>();
        var om = new IdentityMapper<Operator>();
        var uom = new IdentityMapper<UserOperator>();
        var cem = new IdentityMapper<ConjunctionEvent>();
        var am = new IdentityMapper<Alert>();
        var som = new IdentityMapper<SpaceObject>();
        var nsm = new IdentityMapper<NotificationSetting>();
        var dsm = new IdentityMapper<DraftSatellite>();
        var ram = new IdentityMapper<RiskAssessment>();
        var oalm = new IdentityMapper<OperatorActionLog>();

        return new UnitOfWork(
            ctx,
            new SatelliteRepository(ctx, im),
            new DebrisObjectRepository(ctx, dm),
            new OperatorRepository(ctx, om),
            new UserOperatorRepository(ctx, uom),
            new ConjunctionEventRepository(ctx, cem),
            new AlertRepository(ctx, am),
            new SpaceObjectsRepository(ctx, som),
            new NotificationSettingRepository(ctx, nsm),
            new DraftSatelliteRepository(ctx, dsm),
            new RiskAssessmentRepository(ctx, ram),
            new OperatorActionLogRepository(ctx, oalm));
    }

    private static OrbitParameters SampleOrbit() =>
        new(7000e3, 0.001, 51.6, 0, 0, 0);

    [Fact]
    public void DraftSatellites_PropertyAlwaysReturnsSameInstance()
    {
        var uow = CreateUow();

        var first = uow.DraftSatellites;
        var second = uow.DraftSatellites;

        first.Should().BeSameAs(second);
    }

    [Fact]
    public async Task CommitAsync_PersistsAddedDraftSatellite()
    {
        Guid newId;

        var uow = CreateUow();
        var draft = DraftSatellite.Create(TestUsers.UserAId, "Test Sat", null, SampleOrbit());
        newId = draft.Id;

        uow.DraftSatellites.Add(draft);
        var changed = await uow.CommitAsync();

        changed.Should().BeGreaterThan(0);

        await using var verify = CreateContext();
        (await verify.DraftSatellites.AsNoTracking().AnyAsync(d => d.Id == newId)).Should().BeTrue();
    }

    [Fact]
    public async Task CommitAsync_NoChanges_ReturnsZero()
    {
        var uow = CreateUow();

        var changed = await uow.CommitAsync();

        changed.Should().Be(0);
    }
}
