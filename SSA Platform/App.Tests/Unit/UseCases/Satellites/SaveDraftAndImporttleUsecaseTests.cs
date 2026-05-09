using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Application.Mappers.Satellites;
using App.Domain.Entities;
using App.Domain.ValueObjects;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Satellites;

public class SaveDraftSatelliteUseCaseTests
{
    private readonly Mock<IUnitOfWork>               _uow;
    private readonly Mock<IDraftSatelliteRepository> _draftRepo;
    private readonly SaveDraftSatelliteUseCase       _sut;

    public SaveDraftSatelliteUseCaseTests()
    {
        _uow       = new Mock<IUnitOfWork>();
        _draftRepo = new Mock<IDraftSatelliteRepository>();
        _uow.Setup(u => u.DraftSatellites).Returns(_draftRepo.Object);

        var mapper = new DraftSatMapper();
        _sut = new SaveDraftSatelliteUseCase(_uow.Object, mapper);
    }

    private static SaveDraftSatelliteRequest ValidRequest(Guid? id = null) =>
        new SaveDraftSatelliteRequest
        {
            Id                = id,
            Name              = "MyDraft",
            Notes             = "some notes",
            SemiMajorAxis     = 6_778_000,
            Eccentricity      = 0.001,
            Inclination       = 51.6,
            RAAN              = 0,
            ArgumentOfPerigee = 0,
            MeanAnomaly       = 0
        };

    private static DraftSatellite MakeDraft(Guid userId, Guid? draftId = null)
    {
        var draft = DraftSatellite.Create(userId, "OldName", null,
            new OrbitParameters(6_778_000, 0.001, 51.6, 0, 0, 0));
        return draft;
    }

    // ── Create path (no Id in request) ───────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenNoIdInRequest_AddsDraftToRepository()
    {
        var userId  = Guid.NewGuid();
        var request = ValidRequest(id: null);

        var result = await _sut.ExecuteAsync(userId, request);

        _draftRepo.Verify(
            r => r.Add(It.IsAny<DraftSatellite>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoIdInRequest_CommitsAndReturnsDtoWithName()
    {
        var userId  = Guid.NewGuid();
        var request = ValidRequest(id: null);

        var result = await _sut.ExecuteAsync(userId, request);

        _uow.Verify(u => u.CommitAsync(), Times.Once);
        result.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoIdInRequest_ReturnsNonEmptyId()
    {
        var result = await _sut.ExecuteAsync(Guid.NewGuid(), ValidRequest());

        result.Id.Should().NotBe(Guid.Empty);
    }

    // ── Update path (Id present in request) ──────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhenIdInRequestAndDraftFound_DoesNotAddNewDraft()
    {
        var userId  = Guid.NewGuid();
        var draft   = MakeDraft(userId);
        var request = ValidRequest(id: draft.Id);
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draft.Id, userId))
            .ReturnsAsync(draft);

        await _sut.ExecuteAsync(userId, request);

        _draftRepo.Verify(
            r => r.Add(It.IsAny<DraftSatellite>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenIdInRequestAndDraftFound_UpdatesDraftName()
    {
        var userId  = Guid.NewGuid();
        var draft   = MakeDraft(userId);
        var request = ValidRequest(id: draft.Id);
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(draft.Id, userId))
            .ReturnsAsync(draft);

        var result = await _sut.ExecuteAsync(userId, request);

        result.Name.Should().Be("MyDraft");
    }

    [Fact]
    public async Task ExecuteAsync_WhenIdInRequestButDraftNotFound_ThrowsInvalidOperationException()
    {
        var userId    = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(missingId, userId))
            .ReturnsAsync((DraftSatellite?)null);

        Func<Task> act = () => _sut.ExecuteAsync(userId, ValidRequest(id: missingId));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenIdInRequestButDraftNotFound_DoesNotCommit()
    {
        var userId    = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        _draftRepo
            .Setup(r => r.GetByIdAndUserAsync(missingId, userId))
            .ReturnsAsync((DraftSatellite?)null);

        try { await _sut.ExecuteAsync(userId, ValidRequest(id: missingId)); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── DTO mapping ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ReturnsDtoWithCorrectOrbitParameters()
    {
        var userId  = Guid.NewGuid();
        var request = ValidRequest();

        var result = await _sut.ExecuteAsync(userId, request);

        result.Inclination.Should().Be(request.Inclination);
        result.SemiMajorAxis.Should().Be(request.SemiMajorAxis);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class ImportTleFromCelesTrakUseCaseTests
{
    private readonly Mock<IUnitOfWork>             _uow;
    private readonly Mock<ISatelliteRepository>    _satelliteRepo;
    private readonly Mock<ICelesTrakService>       _celestrak;
    private readonly ImportTleFromCelesTrakUseCase _sut;

    public ImportTleFromCelesTrakUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _satelliteRepo = new Mock<ISatelliteRepository>();
        _celestrak     = new Mock<ICelesTrakService>();
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);
        _sut = new ImportTleFromCelesTrakUseCase(_uow.Object, _celestrak.Object);
    }

    private static Satellite MakeSatellite(int noradId = 25544, Guid? operatorId = null)
    {
        var op  = new Operator { Id = operatorId ?? Guid.NewGuid(), Name = "TestCorp" };
        var sat = Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
        sat.NoradId = noradId;
        return sat;
    }

    private static CelesTrakTleDto ValidTleDto() => new CelesTrakTleDto
    {
        Epoch             = DateTime.UtcNow.AddDays(-1),
        Inclination       = 51.6,
        RAAN              = 247.4,
        Eccentricity      = 0.0006703,
        ArgumentOfPerigee = 130.5,
        MeanAnomaly       = 325.0,
        MeanMotion        = 15.5,
        BSTar             = 0.00004,
        FirstDerivative   = 0.00002,
        SecondDerivative  = 0,
        RevolutionNumber  = 42965,
        Line1             = "1 25544U 98067A   24001.50000000  .00002182  00000-0  40768-4 0  9990",
        Line2             = "2 25544  51.6416 247.4627 0006703 130.5360 325.0288 15.50377579429652"
    };

    private void SetupFind(Guid id, Satellite? returns) =>
        _satelliteRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteNotFound_ThrowsException()
    {
        var id = Guid.NewGuid();
        SetupFind(id, null);

        Func<Task> act = () => _sut.ExecuteAsync(id, "user");

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorIdDoesNotMatch_ThrowsException()
    {
        var satellite = MakeSatellite(operatorId: Guid.NewGuid());
        SetupFind(satellite.Id, satellite);

        Func<Task> act = () =>
            _sut.ExecuteAsync(satellite.Id, "user", requiredOperatorId: Guid.NewGuid());

        await act.Should().ThrowAsync<Exception>().WithMessage("*Access denied*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoradIdIsZero_ThrowsException()
    {
        var satellite = MakeSatellite(noradId: 0);
        SetupFind(satellite.Id, satellite);

        Func<Task> act = () => _sut.ExecuteAsync(satellite.Id, "user");

        await act.Should().ThrowAsync<Exception>().WithMessage("*no NORAD ID*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenCelesTrakReturnsNull_ThrowsException()
    {
        var satellite = MakeSatellite(noradId: 25544);
        SetupFind(satellite.Id, satellite);
        _celestrak
            .Setup(c => c.FetchTleByNoradIdAsync(25544))
            .ReturnsAsync((CelesTrakTleDto?)null);

        Func<Task> act = () => _sut.ExecuteAsync(satellite.Id, "user");

        await act.Should().ThrowAsync<Exception>().WithMessage("*CelesTrak*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenGuardFails_DoesNotCommit()
    {
        SetupFind(Guid.NewGuid(), null);

        try { await _sut.ExecuteAsync(Guid.NewGuid(), "user"); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_AddsTleRecord()
    {
        var satellite = MakeSatellite();
        SetupFind(satellite.Id, satellite);
        _celestrak
            .Setup(c => c.FetchTleByNoradIdAsync(satellite.NoradId))
            .ReturnsAsync(ValidTleDto());

        await _sut.ExecuteAsync(satellite.Id, "user");

        _satelliteRepo.Verify(
            r => r.AddTleRecordAsync(It.IsAny<TleRecord>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_CommitsUnitOfWork()
    {
        var satellite = MakeSatellite();
        SetupFind(satellite.Id, satellite);
        _celestrak
            .Setup(c => c.FetchTleByNoradIdAsync(satellite.NoradId))
            .ReturnsAsync(ValidTleDto());

        await _sut.ExecuteAsync(satellite.Id, "user");

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_ReturnsNonEmptyRecordId()
    {
        var satellite = MakeSatellite();
        SetupFind(satellite.Id, satellite);
        _celestrak
            .Setup(c => c.FetchTleByNoradIdAsync(satellite.NoradId))
            .ReturnsAsync(ValidTleDto());

        var id = await _sut.ExecuteAsync(satellite.Id, "user");

        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_TleRecordCreatedByMatchesFetchedBy()
    {
        var satellite = MakeSatellite();
        SetupFind(satellite.Id, satellite);
        _celestrak
            .Setup(c => c.FetchTleByNoradIdAsync(satellite.NoradId))
            .ReturnsAsync(ValidTleDto());

        TleRecord? added = null;
        _satelliteRepo
            .Setup(r => r.AddTleRecordAsync(It.IsAny<TleRecord>()))
            .Callback<TleRecord>(r => added = r)
            .Returns(Task.CompletedTask);

        await _sut.ExecuteAsync(satellite.Id, "operator-user");

        added!.CreatedBy.Should().Be("operator-user");
    }
}