using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Alerts.UseCases;
using App.Application.Mappers.Alerts;
using App.Domain.Entities;
using AwesomeAssertions;
using Base.Contracts;
using Moq;

namespace App.Tests.Unit.UseCases.Alerts;

public class AcknowledgeAlertUseCaseTests : AlertUseCaseTestBase
{
    private readonly AcknowledgeAlertUseCase _sut;

    public AcknowledgeAlertUseCaseTests()
    {
        _sut = new AcknowledgeAlertUseCase(Uow.Object);
    }

    // ── Authorization ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAlertNotFound_ThrowsInvalidOperationException()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());
        AlertRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Alert?)null);

        Func<Task> act = () => _sut.ExecuteAsync(Guid.NewGuid(), operatorId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found or not authorized*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertBelongsToOtherOperator_ThrowsInvalidOperationException()
    {
        var operatorId = Guid.NewGuid();
        var alert      = SetupUnauthorizedAlert(operatorId);

        Func<Task> act = () => _sut.ExecuteAsync(alert.Id, operatorId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthorized_DoesNotCommit()
    {
        var operatorId = Guid.NewGuid();
        var alert      = SetupUnauthorizedAlert(operatorId);

        try { await _sut.ExecuteAsync(alert.Id, operatorId); } catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_AcknowledgesAlert()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);
        AlertRepo
            .Setup(r => r.UpdateAcknowledgedAsync(alert))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(alert.Id, operatorId);

        alert.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_CallsUpdateAcknowledgedAsync()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);
        AlertRepo
            .Setup(r => r.UpdateAcknowledgedAsync(alert))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(alert.Id, operatorId);

        AlertRepo.Verify(
            r => r.UpdateAcknowledgedAsync(alert),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_CommitsUnitOfWork()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);
        AlertRepo
            .Setup(r => r.UpdateAcknowledgedAsync(alert))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(alert.Id, operatorId);

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class GetAlertByIdUseCaseTests : AlertUseCaseTestBase
{
    private readonly GetAlertByIdUseCase _sut;
    private readonly IBaseMapper<AlertDetailsDto, Alert> _mapper;

    public GetAlertByIdUseCaseTests()
    {
        _mapper = new AlertDetailsMapper();
        _sut    = new GetAlertByIdUseCase(Uow.Object, _mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertNotFound_ReturnsNull()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());
        AlertRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Alert?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), operatorId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertBelongsToOtherOperator_ReturnsNull()
    {
        var operatorId = Guid.NewGuid();
        var alert      = SetupUnauthorizedAlert(operatorId);

        var result = await _sut.ExecuteAsync(alert.Id, operatorId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_ReturnsDtoWithCorrectId()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);

        var result = await _sut.ExecuteAsync(alert.Id, operatorId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(alert.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_MapsSeverityCorrectly()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);

        var result = await _sut.ExecuteAsync(alert.Id, operatorId);

        result!.Severity.Should().Be(alert.Severity);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_MapsIsAcknowledgedCorrectly()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);

        var result = await _sut.ExecuteAsync(alert.Id, operatorId);

        result!.IsAcknowledged.Should().Be(alert.IsAcknowledged);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertHasNoConjunctionEvent_ConjunctionEventDtoIsNull()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);
        alert.ConjunctionEvent = null;

        var result = await _sut.ExecuteAsync(alert.Id, operatorId);

        result!.ConjunctionEvent.Should().BeNull();
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class GetOperatorAlertsUseCaseTests : AlertUseCaseTestBase
{
    private readonly GetOperatorAlertsUseCase _sut;

    public GetOperatorAlertsUseCaseTests()
    {
        var mapper = new AlertSummaryMapper();
        _sut = new GetOperatorAlertsUseCase(Uow.Object, mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoAlerts_ReturnsEmptyList()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert>());

        var result = await _sut.ExecuteAsync(operatorId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDtoForEachAlert()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        var alerts      = new List<Alert>
        {
            MakeAlert(conjunction.Id),
            MakeAlert(conjunction.Id)
        };

        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent> { conjunction });
        AlertRepo
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(alerts);

        var result = await _sut.ExecuteAsync(operatorId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_MapsAlertIdToDto()
    {
        var operatorId  = Guid.NewGuid();
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
            .Setup(r => r.GetByConjunctionIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Alert> { alert });

        var result = await _sut.ExecuteAsync(operatorId);

        result.Single().Id.Should().Be(alert.Id);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class UpdateAlertMessageUseCaseTests : AlertUseCaseTestBase
{
    private readonly UpdateAlertMessageUseCase _sut;

    public UpdateAlertMessageUseCaseTests()
    {
        _sut = new UpdateAlertMessageUseCase(Uow.Object);
    }

    // ── Authorization ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAlertNotFound_ThrowsInvalidOperationException()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());
        AlertRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((Alert?)null);

        Func<Task> act = () => _sut.ExecuteAsync(Guid.NewGuid(), operatorId, "new message");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found or not authorized*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlertBelongsToOtherOperator_ThrowsInvalidOperationException()
    {
        var operatorId = Guid.NewGuid();
        var alert      = SetupUnauthorizedAlert(operatorId);

        Func<Task> act = () => _sut.ExecuteAsync(alert.Id, operatorId, "new message");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthorized_DoesNotCommit()
    {
        var operatorId = Guid.NewGuid();
        var alert      = SetupUnauthorizedAlert(operatorId);

        try { await _sut.ExecuteAsync(alert.Id, operatorId, "msg"); } catch { }

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_CallsUpdateMessageAsync()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);
        AlertRepo
            .Setup(r => r.UpdateMessageAsync(alert))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(alert.Id, operatorId, "updated message");

        AlertRepo.Verify(r => r.UpdateMessageAsync(alert), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorized_CommitsUnitOfWork()
    {
        var operatorId    = Guid.NewGuid();
        var (_, _, alert) = SetupAuthorizedAlert(operatorId);
        AlertRepo
            .Setup(r => r.UpdateMessageAsync(alert))
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(alert.Id, operatorId, "updated message");

        Uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}