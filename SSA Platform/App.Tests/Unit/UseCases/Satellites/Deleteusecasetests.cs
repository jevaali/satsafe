using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.ValueObjects;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Satellites;

public class DeleteSatelliteUseCaseTests
{
    private readonly Mock<IUnitOfWork>          _uow;
    private readonly Mock<ISatelliteRepository> _satelliteRepo;
    private readonly DeleteSatelliteUseCase     _sut;

    public DeleteSatelliteUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _satelliteRepo = new Mock<ISatelliteRepository>();
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);
        _sut = new DeleteSatelliteUseCase(_uow.Object);
    }

    private static Satellite MakeSatellite(Guid? operatorId = null)
    {
        var op = new Operator { Id = operatorId ?? Guid.NewGuid(), Name = "TestCorp" };
        return Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
    }

    private void SetupFind(Guid id, Satellite? returns) =>
        _satelliteRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    // ── Not found / already deleted ───────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenSatelliteNotFound_ThrowsException()
    {
        var id = Guid.NewGuid();
        SetupFind(id, null);

        Func<Task> act = () => _sut.Execute(id, Guid.NewGuid(), "admin");

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Execute_WhenSatelliteAlreadyDeleted_ThrowsException()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        satellite.SoftDelete("previous-admin");
        SetupFind(satellite.Id, satellite);

        Func<Task> act = () => _sut.Execute(satellite.Id, opId, "admin");

        await act.Should().ThrowAsync<Exception>().WithMessage("*already deleted*");
    }

    [Fact]
    public async Task Execute_WhenSatelliteNotFound_DoesNotCommit()
    {
        SetupFind(Guid.NewGuid(), null);

        try { await _sut.Execute(Guid.NewGuid(), Guid.NewGuid(), "admin"); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Authorization ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WhenOperatorIdDoesNotMatch_ThrowsUnauthorizedAccessException()
    {
        var ownerOpId  = Guid.NewGuid();
        var callerOpId = Guid.NewGuid();
        var satellite  = MakeSatellite(ownerOpId);
        SetupFind(satellite.Id, satellite);

        Func<Task> act = () => _sut.Execute(satellite.Id, callerOpId, "admin");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Execute_WhenOperatorIdDoesNotMatch_DoesNotCommit()
    {
        var satellite = MakeSatellite(Guid.NewGuid());
        SetupFind(satellite.Id, satellite);

        try { await _sut.Execute(satellite.Id, Guid.NewGuid(), "admin"); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WithValidRequest_SoftDeletesSatellite()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        SetupFind(satellite.Id, satellite);
        _satelliteRepo
            .Setup(r => r.UpdateAsync(satellite, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        await _sut.Execute(satellite.Id, opId, "admin");

        satellite.DeletedAt.Should().NotBeNull();
        satellite.DeletedBy.Should().Be("admin");
    }

    [Fact]
    public async Task Execute_WithValidRequest_CallsUpdateAsync()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        SetupFind(satellite.Id, satellite);
        _satelliteRepo
            .Setup(r => r.UpdateAsync(satellite, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        await _sut.Execute(satellite.Id, opId, "admin");

        _satelliteRepo.Verify(
            r => r.UpdateAsync(satellite, It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CommitsUnitOfWork()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        SetupFind(satellite.Id, satellite);
        _satelliteRepo
            .Setup(r => r.UpdateAsync(satellite, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        await _sut.Execute(satellite.Id, opId, "admin");

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}

public class DeleteDraftSatelliteUseCaseTests
{
    private readonly Mock<IUnitOfWork>              _uow;
    private readonly Mock<IDraftSatelliteRepository> _draftRepo;
    private readonly DeleteDraftSatelliteUseCase    _sut;

    public DeleteDraftSatelliteUseCaseTests()
    {
        _uow       = new Mock<IUnitOfWork>();
        _draftRepo = new Mock<IDraftSatelliteRepository>();
        _uow.Setup(u => u.DraftSatellites).Returns(_draftRepo.Object);
        _sut = new DeleteDraftSatelliteUseCase(_uow.Object);
    }

    private static DraftSatellite MakeDraft(Guid userId) =>
        DraftSatellite.Create(userId, "TestDraft", null,
            new OrbitParameters(6_778_000, 0.001, 51.6, 0, 0, 0));

    // ── Not found / not owned ─────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenDraftNotFound_ReturnsFalse()
    {
        var userId  = Guid.NewGuid();
        var draftId = Guid.NewGuid();
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draftId, userId))
            .ReturnsAsync((DraftSatellite?)null);

        var result = await _sut.ExecuteAsync(userId, draftId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDraftNotFound_DoesNotCommit()
    {
        var userId  = Guid.NewGuid();
        var draftId = Guid.NewGuid();
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draftId, userId))
            .ReturnsAsync((DraftSatellite?)null);

        await _sut.ExecuteAsync(userId, draftId);

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenDraftFound_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var draft  = MakeDraft(userId);
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draft.Id, userId))
            .ReturnsAsync(draft);

        var result = await _sut.ExecuteAsync(userId, draft.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDraftFound_RemovesDraft()
    {
        var userId = Guid.NewGuid();
        var draft  = MakeDraft(userId);
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draft.Id, userId))
            .ReturnsAsync(draft);

        await _sut.ExecuteAsync(userId, draft.Id);

        _draftRepo.Verify(r => r.Remove(draft), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDraftFound_CommitsUnitOfWork()
    {
        var userId = Guid.NewGuid();
        var draft  = MakeDraft(userId);
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draft.Id, userId))
            .ReturnsAsync(draft);

        await _sut.ExecuteAsync(userId, draft.Id);

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }
}