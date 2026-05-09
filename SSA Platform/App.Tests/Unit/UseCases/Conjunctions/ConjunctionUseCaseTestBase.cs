using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public abstract class ConjunctionUseCaseTestBase
{
    protected readonly Mock<IUnitOfWork>                 Uow;
    protected readonly Mock<ISatelliteRepository>        SatelliteRepo;
    protected readonly Mock<IConjunctionEventRepository> ConjunctionRepo;
    protected readonly Mock<IAlertRepository>            AlertRepo;
    protected readonly Mock<INotificationSettingRepository> NotificationRepo;
    protected readonly Mock<ISpaceObjectsRepository>     SpaceObjectsRepo;

    protected ConjunctionUseCaseTestBase()
    {
        Uow              = new Mock<IUnitOfWork>();
        SatelliteRepo    = new Mock<ISatelliteRepository>();
        ConjunctionRepo  = new Mock<IConjunctionEventRepository>();
        AlertRepo        = new Mock<IAlertRepository>();
        NotificationRepo = new Mock<INotificationSettingRepository>();
        SpaceObjectsRepo = new Mock<ISpaceObjectsRepository>();

        Uow.Setup(u => u.Satellites).Returns(SatelliteRepo.Object);
        Uow.Setup(u => u.ConjunctionEvents).Returns(ConjunctionRepo.Object);
        Uow.Setup(u => u.Alerts).Returns(AlertRepo.Object);
        Uow.Setup(u => u.NotificationSettings).Returns(NotificationRepo.Object);
        Uow.Setup(u => u.SpaceObjects).Returns(SpaceObjectsRepo.Object);
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    protected static Satellite MakeSatellite(Guid operatorId)
    {
        var op = new Operator { Id = operatorId, Name = "TestCorp" };
        return Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
    }

    protected static ConjunctionEvent MakeConjunction(
        Guid primaryId, Guid secondaryId,
        double missDistance = 5_000,
        DateTime? tca = null) =>
        new ConjunctionEvent(primaryId, secondaryId,
            tca ?? DateTime.UtcNow.AddHours(48), missDistance, 10_000);

    protected static Alert MakeAlert(Guid conjunctionId, bool acknowledged = false) =>
        new Alert
        {
            Id                 = Guid.NewGuid(),
            ConjunctionEventId = conjunctionId,
            UserId             = Guid.NewGuid(),
            Severity           = EAlertSeverity.High,
            IsAcknowledged     = acknowledged,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = "system"
        };
}

