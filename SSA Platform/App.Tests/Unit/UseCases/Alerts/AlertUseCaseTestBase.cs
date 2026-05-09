using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;

using Moq;


namespace App.Tests.Unit.UseCases.Alerts;

public abstract class AlertUseCaseTestBase
{
    protected readonly Mock<IUnitOfWork>                  Uow;
    protected readonly Mock<ISatelliteRepository>         SatelliteRepo;
    protected readonly Mock<IConjunctionEventRepository>  ConjunctionRepo;
    protected readonly Mock<IAlertRepository>             AlertRepo;

    protected AlertUseCaseTestBase()
    {
        Uow             = new Mock<IUnitOfWork>();
        SatelliteRepo   = new Mock<ISatelliteRepository>();
        ConjunctionRepo = new Mock<IConjunctionEventRepository>();
        AlertRepo       = new Mock<IAlertRepository>();

        Uow.Setup(u => u.Satellites).Returns(SatelliteRepo.Object);
        Uow.Setup(u => u.ConjunctionEvents).Returns(ConjunctionRepo.Object);
        Uow.Setup(u => u.Alerts).Returns(AlertRepo.Object);
    }

    // ── Factory helpers ───────────────────────────────────────────────────────

    protected static Satellite MakeSatellite(Guid operatorId)
    {
        var op = new Operator { Id = operatorId, Name = "TestCorp" };
        return Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
    }

    protected static ConjunctionEvent MakeConjunction(Guid primaryId, Guid secondaryId) =>
        new ConjunctionEvent(primaryId, secondaryId,
            DateTime.UtcNow.AddHours(48), 5_000, 10_000);

    protected static Alert MakeAlert(Guid conjunctionEventId, bool acknowledged = false) =>
        new Alert
        {
            Id                 = Guid.NewGuid(),
            ConjunctionEventId = conjunctionEventId,
            UserId             = Guid.NewGuid(),
            Severity           = EAlertSeverity.High,
            IsAcknowledged     = acknowledged,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = "system"
        };

    /// <summary>
    /// Sets up the full authorization chain so the alert is reachable.
    /// Returns the satellite, conjunction and alert created.
    /// </summary>
    protected (Satellite Satellite, ConjunctionEvent Conjunction, Alert Alert)
        SetupAuthorizedAlert(Guid operatorId)
    {
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        var alert       = MakeAlert(conjunction.Id);

        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent> { conjunction });
        AlertRepo
            .Setup(r => r.FindAsync(alert.Id, It.IsAny<Guid>()))
            .ReturnsAsync(alert);

        return (satellite, conjunction, alert);
    }

    /// <summary>
    /// Sets up the chain so the alert is NOT reachable (belongs to another operator).
    /// </summary>
    protected Alert SetupUnauthorizedAlert(Guid operatorId)
    {
        var alert = MakeAlert(Guid.NewGuid()); // conjunctionId not in operator's list

        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());
        AlertRepo
            .Setup(r => r.FindAsync(alert.Id, It.IsAny<Guid>()))
            .ReturnsAsync(alert);

        return alert;
    }
}