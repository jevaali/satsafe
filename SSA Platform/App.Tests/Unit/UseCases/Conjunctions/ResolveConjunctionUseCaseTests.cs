using App.Application.Features.Conjunctions.UseCases;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public class ResolveConjunctionUseCaseTests : ConjunctionUseCaseTestBase
{
    private readonly ResolveConjunctionUseCase _sut;

    public ResolveConjunctionUseCaseTests()
    {
        _sut = new ResolveConjunctionUseCase(Uow.Object);
    }

    private void SetupConjunction(Guid id, ConjunctionEvent? returns) =>
        ConjunctionRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    // ── Guard clauses ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenConjunctionNotFound_ThrowsInvalidOperationException()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        SetupConjunction(Guid.NewGuid(), null);

        Func<Task> act = () => _sut.ExecuteAsync(Guid.NewGuid(), operatorId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found or not authorized*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsNeitherObject_ThrowsInvalidOperationException()
    {
        var operatorId  = Guid.NewGuid();
        var conjunction = MakeConjunction(Guid.NewGuid(), Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        SetupConjunction(conjunction.Id, conjunction);

        Func<Task> act = () => _sut.ExecuteAsync(conjunction.Id, operatorId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthorized_DoesNotCommit()
    {
        var operatorId  = Guid.NewGuid();
        var conjunction = MakeConjunction(Guid.NewGuid(), Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        SetupConjunction(conjunction.Id, conjunction);

        try { await _sut.ExecuteAsync(conjunction.Id, operatorId); } catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_MarksConjunctionAsResolved()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        SetupConjunction(conjunction.Id, conjunction);
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert>());

        await _sut.ExecuteAsync(conjunction.Id, operatorId);

        conjunction.Status.Should().Be(EConjunctionEventStatus.Resolved);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_AcknowledgesOpenAlerts()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        SetupConjunction(conjunction.Id, conjunction);
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        var openAlert = MakeAlert(conjunction.Id, acknowledged: false);
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert> { openAlert });
        AlertRepo
            .Setup(r => r.UpdateAsync(openAlert, It.IsAny<Guid>()))
            .ReturnsAsync(openAlert);

        await _sut.ExecuteAsync(conjunction.Id, operatorId);

        openAlert.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_DoesNotAcknowledgeAlreadyAcknowledgedAlerts()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        SetupConjunction(conjunction.Id, conjunction);
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        var alreadyAcknowledged = MakeAlert(conjunction.Id, acknowledged: true);
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert> { alreadyAcknowledged });

        await _sut.ExecuteAsync(conjunction.Id, operatorId);

        AlertRepo.Verify(
            r => r.UpdateAsync(alreadyAcknowledged, It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_CommitsUnitOfWork()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        SetupConjunction(conjunction.Id, conjunction);
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(conjunction, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert>());

        await _sut.ExecuteAsync(conjunction.Id, operatorId);

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}
