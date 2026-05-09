using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Satellites;

public class AddSatelliteUseCaseTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork>          _uow;
    private readonly Mock<IOperatorRepository>  _operatorRepo;
    private readonly Mock<ISatelliteRepository> _satelliteRepo;
    private readonly AddSatelliteUseCase        _sut;

    public AddSatelliteUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _operatorRepo  = new Mock<IOperatorRepository>();
        _satelliteRepo = new Mock<ISatelliteRepository>();

        _uow.Setup(u => u.Operators).Returns(_operatorRepo.Object);
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);

        _sut = new AddSatelliteUseCase(_uow.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Operator MakeOperator() =>
        new Operator { Id = Guid.NewGuid(), Name = "TestCorp" };

    private static AddSatelliteRequest ValidRequest(Guid? operatorId = null) =>
        new AddSatelliteRequest
        {
            OperatorId        = operatorId ?? Guid.NewGuid(),
            Name              = "TestSat",
            LaunchDate        = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            NoradId           = 99999,
            SemiMajorAxis     = 6_778_000,
            Eccentricity      = 0.001,
            Inclination       = 51.6,
            RAAN              = 0,
            ArgumentOfPerigee = 0,
            MeanAnomaly       = 0
        };

    // FindAsync has an optional second parameter (appUserId).
    // Moq cannot build expression trees for methods with optional params when
    // only the required param is specified — always pass It.IsAny<Guid>() for
    // the optional one to avoid the "expression tree with optional args" error.
    private void SetupFindOperator(Guid operatorId, Operator? returns) =>
        _operatorRepo
            .Setup(r => r.FindAsync(operatorId, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    // ── Operator lookup ───────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_Always_LooksUpOperatorByRequestOperatorId()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        await _sut.Execute(request);

        _operatorRepo.Verify(
            r => r.FindAsync(request.OperatorId, It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WhenOperatorNotFound_ThrowsException()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, null);

        Func<Task> act = () => _sut.Execute(request);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Execute_WhenOperatorNotFound_DoesNotAddSatellite()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, null);

        try { await _sut.Execute(request); } catch { /* expected */ }

        _satelliteRepo.Verify(
            r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_WhenOperatorNotFound_DoesNotCommit()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, null);

        try { await _sut.Execute(request); } catch { /* expected */ }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WithValidRequest_AddsSatelliteToRepository()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        await _sut.Execute(request);

        _satelliteRepo.Verify(
            r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CommitsUnitOfWork()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        await _sut.Execute(request);

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_ReturnsNonEmptyGuid()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        var id = await _sut.Execute(request);

        id.Should().NotBe(Guid.Empty);
    }

    // ── Satellite built with correct data ─────────────────────────────────────

    private Satellite CaptureAddedSatellite(AddSatelliteRequest request)
    {
        Satellite? captured = null;
        _satelliteRepo
            .Setup(r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .Callback<Satellite, Guid>((s, _) => captured = s);
        SetupFindOperator(request.OperatorId, MakeOperator());
        return captured!; // populated after Execute() is called
    }

    [Fact]
    public async Task Execute_WithValidRequest_AddsSatelliteWithCorrectName()
    {
        var request  = ValidRequest();
        var op       = MakeOperator();
        SetupFindOperator(request.OperatorId, op);

        Satellite? added = null;
        _satelliteRepo
            .Setup(r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .Callback<Satellite, Guid>((s, _) => added = s);

        await _sut.Execute(request);

        added!.Name.Translate().Should().Be(request.Name);
    }

    [Fact]
    public async Task Execute_WithValidRequest_AddsSatelliteWithCorrectNoradId()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        Satellite? added = null;
        _satelliteRepo
            .Setup(r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .Callback<Satellite, Guid>((s, _) => added = s);

        await _sut.Execute(request);

        added!.NoradId.Should().Be(request.NoradId!.Value);
    }

    [Fact]
    public async Task Execute_WithValidRequest_AddsSatelliteWithOrbitSet()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        Satellite? added = null;
        _satelliteRepo
            .Setup(r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .Callback<Satellite, Guid>((s, _) => added = s);

        await _sut.Execute(request);

        added!.Orbit.Should().NotBeNull();
        added.Orbit!.Parameters.Inclination.Should().Be(request.Inclination!.Value);
    }

    [Fact]
    public async Task Execute_WithValidRequest_NullsOutOperatorNavigationProperty()
    {
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, MakeOperator());

        Satellite? added = null;
        _satelliteRepo
            .Setup(r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .Callback<Satellite, Guid>((s, _) => added = s);

        await _sut.Execute(request);

        added!.Operator.Should().BeNull();
    }

    [Fact]
    public async Task Execute_WithValidRequest_OperatorIdFkRemainsSetAfterNullingNavProperty()
    {
        var request = ValidRequest();
        var op      = MakeOperator();
        SetupFindOperator(request.OperatorId, op);

        Satellite? added = null;
        _satelliteRepo
            .Setup(r => r.Add(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .Callback<Satellite, Guid>((s, _) => added = s);

        await _sut.Execute(request);

        added!.OperatorId.Should().Be(op.Id);
    }
}