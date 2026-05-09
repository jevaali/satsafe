using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Satellites;

public class AddTleManuallyUseCaseTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork>          _uow;
    private readonly Mock<ISatelliteRepository> _satelliteRepo;
    private readonly AddTleManuallyUseCase      _sut;

    public AddTleManuallyUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _satelliteRepo = new Mock<ISatelliteRepository>();
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);
        _sut = new AddTleManuallyUseCase(_uow.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    // Satellite has a private constructor and private setters —
    // use the public factory methods instead of subclassing.
    private static Satellite MakeSatellite(Guid? operatorId = null)
    {
        var op = new Operator { Id = operatorId ?? Guid.NewGuid(), Name = "TestCorp" };
        return Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
    }

    private void SetupFindSatellite(Guid id, Satellite? returns) =>
        _satelliteRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    // Real ISS TLE lines — 69 characters each, valid format
    private const string ValidLine1 =
        "1 25544U 98067A   24001.50000000  .00002182  00000-0  40768-4 0  9990";
    private const string ValidLine2 =
        "2 25544  51.6416 247.4627 0006703 130.5360 325.0288 15.50377579429652";

    // ── Satellite lookup ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteNotFound_ThrowsException()
    {
        var id = Guid.NewGuid();
        SetupFindSatellite(id, null);

        Func<Task> act = () => _sut.ExecuteAsync(id, ValidLine1, ValidLine2, "user");

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteNotFound_DoesNotCommit()
    {
        var id = Guid.NewGuid();
        SetupFindSatellite(id, null);

        try { await _sut.ExecuteAsync(id, ValidLine1, ValidLine2, "user"); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Operator access check ─────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenRequiredOperatorIdDoesNotMatch_ThrowsException()
    {
        var ownerOpId  = Guid.NewGuid();
        var callerOpId = Guid.NewGuid();
        var satellite  = MakeSatellite(operatorId: ownerOpId);
        SetupFindSatellite(satellite.Id, satellite);

        Func<Task> act = () =>
            _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user",
                requiredOperatorId: callerOpId);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Access denied*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequiredOperatorIdMatches_DoesNotThrow()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(operatorId: opId);
        SetupFindSatellite(satellite.Id, satellite);

        Func<Task> act = () =>
            _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user",
                requiredOperatorId: opId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenRequiredOperatorIdIsNull_SkipsAccessCheck()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        Func<Task> act = () =>
            _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user",
                requiredOperatorId: null);

        await act.Should().NotThrowAsync();
    }

    // ── TLE parsing ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithInvalidTleLines_ThrowsException()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        Func<Task> act = () =>
            _sut.ExecuteAsync(satellite.Id, "bad line 1", "bad line 2", "user");

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid TLE*");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidTle_DoesNotCommit()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        try { await _sut.ExecuteAsync(satellite.Id, "bad", "bad", "user"); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WithValidTle_AddsTleRecord()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        await _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user");

        _satelliteRepo.Verify(
            r => r.AddTleRecordAsync(It.IsAny<TleRecord>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTle_CommitsUnitOfWork()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        await _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user");

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTle_TleRecordHasCorrectRawLines()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        TleRecord? added = null;
        _satelliteRepo
            .Setup(r => r.AddTleRecordAsync(It.IsAny<TleRecord>()))
            .Callback<TleRecord>(r => added = r)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user");

        added!.RawLine1.Should().Be(ValidLine1);
        added.RawLine2.Should().Be(ValidLine2);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTle_TleRecordCreatedByMatchesAddedBy()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        TleRecord? added = null;
        _satelliteRepo
            .Setup(r => r.AddTleRecordAsync(It.IsAny<TleRecord>()))
            .Callback<TleRecord>(r => added = r)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "operator-user");

        added!.CreatedBy.Should().Be("operator-user");
    }

    [Fact]
    public async Task ExecuteAsync_WithValidTle_TleRecordSpaceObjectIdMatchesSatelliteId()
    {
        var satellite = MakeSatellite();
        SetupFindSatellite(satellite.Id, satellite);

        TleRecord? added = null;
        _satelliteRepo
            .Setup(r => r.AddTleRecordAsync(It.IsAny<TleRecord>()))
            .Callback<TleRecord>(r => added = r)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(satellite.Id, ValidLine1, ValidLine2, "user");

        added!.SpaceObjectId.Should().Be(satellite.Id);
    }
}