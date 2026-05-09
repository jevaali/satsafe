using App.Application.Features.Conjunctions.UseCases;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public class RefreshConjunctionStatusesUseCaseTests : ConjunctionUseCaseTestBase
{
    private readonly RefreshConjunctionStatusesUseCase _sut;

    public RefreshConjunctionStatusesUseCaseTests()
    {
        _sut = new RefreshConjunctionStatusesUseCase(Uow.Object);
    }

    private void SetupOperator(Guid operatorId, IEnumerable<ConjunctionEvent> conjunctions,
        Satellite? satellite = null)
    {
        var sat = satellite ?? MakeSatellite(operatorId);
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { sat });
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(conjunctions.ToList());
        ConjunctionRepo
            .Setup(r => r.UpdateAsync(It.IsAny<ConjunctionEvent>(), It.IsAny<Guid>()))
            .ReturnsAsync((ConjunctionEvent c, Guid _) => c);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoConjunctions_CommitsOnce()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CallsUpdateForEachConjunction()
    {
        var operatorId   = Guid.NewGuid();
        var satellite    = MakeSatellite(operatorId);
        var conjunctions = new List<ConjunctionEvent>
        {
            MakeConjunction(satellite.Id, Guid.NewGuid(), tca: DateTime.UtcNow.AddHours(100)),
            MakeConjunction(satellite.Id, Guid.NewGuid(), tca: DateTime.UtcNow.AddHours(100))
        };
        SetupOperator(operatorId, conjunctions, satellite);

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        ConjunctionRepo.Verify(
            r => r.UpdateAsync(It.IsAny<ConjunctionEvent>(), It.IsAny<Guid>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WhenConjunctionBecomesImminent_AddsImminentAlert()
    {
        var operatorId = Guid.NewGuid();
        var satellite  = MakeSatellite(operatorId);

        // TCA < 24h away — RefreshStatus() will set Imminent
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid(),
            tca: DateTime.UtcNow.AddHours(12));
        // Conjunction starts as Pending so it transitions to Imminent
        SetupOperator(operatorId, new List<ConjunctionEvent> { conjunction }, satellite);

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        AlertRepo.Verify(
            r => r.Add(It.Is<Alert>(a => a.Severity == EAlertSeverity.Critical), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConjunctionAlreadyImminent_DoesNotAddDuplicateAlert()
    {
        var operatorId = Guid.NewGuid();
        var satellite  = MakeSatellite(operatorId);

        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid(),
            tca: DateTime.UtcNow.AddHours(12));

        // Simulate already having a critical unacknowledged alert
        var existingAlert = new Alert
        {
            Id = Guid.NewGuid(), ConjunctionEventId = conjunction.Id,
            Severity = EAlertSeverity.Critical, IsAcknowledged = false,
            UserId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, CreatedBy = "system"
        };
        conjunction.Alerts.Add(existingAlert);

        // Force status to already be Imminent so the transition doesn't fire
        conjunction.RefreshStatus(); // sets Imminent
        SetupOperator(operatorId, new List<ConjunctionEvent> { conjunction }, satellite);

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        AlertRepo.Verify(
            r => r.Add(It.IsAny<Alert>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConjunctionAlreadyOccurred_StatusBecomesAlreadyOccurred()
    {
        var operatorId = Guid.NewGuid();
        var satellite  = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid(),
            tca: DateTime.UtcNow.AddHours(-1)); // TCA in past
        SetupOperator(operatorId, new List<ConjunctionEvent> { conjunction }, satellite);

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        conjunction.Status.Should().Be(EConjunctionEventStatus.AlreadyOccurred);
    }
}
