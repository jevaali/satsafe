using App.Application.Features.Conjunctions.UseCases;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public class AcknowledgeConjunctionSideUseCaseTests : ConjunctionUseCaseTestBase
{
    private readonly AcknowledgeConjunctionSideUseCase _sut;

    public AcknowledgeConjunctionSideUseCaseTests()
    {
        _sut = new AcknowledgeConjunctionSideUseCase(Uow.Object);
    }

    private void SetupConjunction(Guid conjunctionId, ConjunctionEvent? returns) =>
        ConjunctionRepo
            .Setup(r => r.FindAsync(conjunctionId, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    private void SetupSatellites(Guid operatorId, IEnumerable<Satellite> satellites) =>
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(satellites.ToList());

    // ── Guard clauses ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenConjunctionNotFound_ThrowsException()
    {
        SetupConjunction(Guid.NewGuid(), null);
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite>());

        Func<Task> act = () =>
            _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), EOperatorAcknowledgement.Confirmed);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsNeitherObject_ThrowsUnauthorizedAccessException()
    {
        var operatorId  = Guid.NewGuid();
        var conjunction = MakeConjunction(Guid.NewGuid(), Guid.NewGuid());
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite>()); // no satellites

        Func<Task> act = () =>
            _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Confirmed);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthorized_DoesNotCommit()
    {
        var operatorId  = Guid.NewGuid();
        var conjunction = MakeConjunction(Guid.NewGuid(), Guid.NewGuid());
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite>());

        try { await _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Confirmed); }
        catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Primary side ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsPrimary_AcknowledgesPrimarySide()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert>());

        await _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Confirmed);

        conjunction.PrimaryAcknowledgement.Should().Be(EOperatorAcknowledgement.Confirmed);
    }

    // ── Secondary side ────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsSecondary_AcknowledgesSecondarySide()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(Guid.NewGuid(), satellite.Id);
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert>());

        await _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Confirmed);

        conjunction.SecondaryAcknowledgement.Should().Be(EOperatorAcknowledgement.Confirmed);
    }

    // ── Resolved state acknowledges open alerts ───────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenStateIsResolved_AcknowledgesOpenAlerts()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        // Move to Acknowledged first so Resolved is a valid forward transition
        conjunction.AcknowledgePrimarySide(EOperatorAcknowledgement.Confirmed);

        var openAlert = MakeAlert(conjunction.Id, acknowledged: false);
        conjunction.Alerts.Add(openAlert);

        await _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Resolved);

        openAlert.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenStateIsNotResolved_DoesNotTouchAlerts()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        await _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Confirmed);

        AlertRepo.Verify(
            r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_CommitsUnitOfWork()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SetupConjunction(conjunction.Id, conjunction);
        SetupSatellites(operatorId, new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        await _sut.ExecuteAsync(conjunction.Id, operatorId, EOperatorAcknowledgement.Confirmed);

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}