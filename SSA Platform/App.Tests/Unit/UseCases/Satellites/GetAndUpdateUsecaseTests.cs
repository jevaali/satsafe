using App.Application.Features.Satellites.DTOs;
using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Application.Mappers.RiskAssessments;
using App.Application.Mappers.Satellites;
using App.Application.Mappers.Shared;
using App.Domain.Entities;
using App.Domain.ValueObjects;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Satellites;

public class GetSatelliteByIdUseCaseTests
{
    private readonly Mock<IUnitOfWork>          _uow;
    private readonly Mock<ISatelliteRepository> _satelliteRepo;
    private readonly GetSatelliteByIdUseCase    _sut;

    public GetSatelliteByIdUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _satelliteRepo = new Mock<ISatelliteRepository>();
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);

        var orbitMapper = new OrbitMapper();
        var riskMapper  = new RiskMapper();
        var tleMapper   = new TleRecordSummaryMapper();
        var mapper      = new SatelliteDetailMapper(orbitMapper, riskMapper, tleMapper);

        _sut = new GetSatelliteByIdUseCase(_uow.Object, mapper);
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

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteNotFound_ReturnsNull()
    {
        SetupFind(Guid.NewGuid(), null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorIdDoesNotMatch_ReturnsNull()
    {
        var satellite = MakeSatellite(operatorId: Guid.NewGuid());
        SetupFind(satellite.Id, satellite);

        var result = await _sut.ExecuteAsync(satellite.Id, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenFound_ReturnsDtoWithCorrectId()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        SetupFind(satellite.Id, satellite);

        var result = await _sut.ExecuteAsync(satellite.Id, opId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(satellite.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFound_ReturnsDtoWithCorrectName()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        SetupFind(satellite.Id, satellite);

        var result = await _sut.ExecuteAsync(satellite.Id, opId);

        result!.Name.Should().Be(satellite.Name.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteHasNoOrbit_ReturnsNullOrbit()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        SetupFind(satellite.Id, satellite);

        var result = await _sut.ExecuteAsync(satellite.Id, opId);

        result!.Orbit.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteHasOrbit_MapsOrbitParameters()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        satellite.SetOrbit(new OrbitParameters(6_778_000, 0.001, 51.6, 0, 0, 0));
        SetupFind(satellite.Id, satellite);

        var result = await _sut.ExecuteAsync(satellite.Id, opId);

        result!.Orbit.Should().NotBeNull();
        result.Orbit!.Inclination.Should().Be(51.6);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSatelliteHasTleRecords_OrdersByEpochDescending()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        var older = new TleRecord
        {
            Id = Guid.NewGuid(), SpaceObject = satellite, SpaceObjectId = satellite.Id,
            Epoch = DateTime.UtcNow.AddDays(-5), RawLine1 = "", RawLine2 = "",
            CreatedBy = "system", CreatedAt = DateTime.UtcNow
        };
        var newer = new TleRecord
        {
            Id = Guid.NewGuid(), SpaceObject = satellite, SpaceObjectId = satellite.Id,
            Epoch = DateTime.UtcNow.AddDays(-1), RawLine1 = "", RawLine2 = "",
            CreatedBy = "system", CreatedAt = DateTime.UtcNow
        };
        satellite.TleRecords.Add(older);
        satellite.TleRecords.Add(newer);
        SetupFind(satellite.Id, satellite);

        var result = await _sut.ExecuteAsync(satellite.Id, opId);

        result!.TleRecords.First().Id.Should().Be(newer.Id);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class GetOperatorSatellitesUseCaseTests
{
    private readonly Mock<IUnitOfWork>            _uow;
    private readonly Mock<ISatelliteRepository>   _satelliteRepo;
    private readonly GetOperatorSatellitesUseCase _sut;

    public GetOperatorSatellitesUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _satelliteRepo = new Mock<ISatelliteRepository>();
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);

        var mapper = new SatelliteSummaryMapper(new OrbitMapper());
        _sut = new GetOperatorSatellitesUseCase(_uow.Object, mapper);
    }

    private static Satellite MakeSatellite(Guid operatorId)
    {
        var op = new Operator { Id = operatorId, Name = "TestCorp" };
        return Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoSatellites_ReturnsEmptyList()
    {
        var opId = Guid.NewGuid();
        _satelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(opId))
            .ReturnsAsync(new List<Satellite>());

        var result = await _sut.ExecuteAsync(opId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDtoForEachSatellite()
    {
        var opId = Guid.NewGuid();
        var satellites = new List<Satellite>
        {
            MakeSatellite(opId),
            MakeSatellite(opId)
        };
        _satelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(opId))
            .ReturnsAsync(satellites);

        var result = await _sut.ExecuteAsync(opId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_MapsSatelliteIdToDto()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        _satelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(opId))
            .ReturnsAsync(new List<Satellite> { satellite });

        var result = await _sut.ExecuteAsync(opId);

        result.Single().Id.Should().Be(satellite.Id);
    }
}

// ═════════════════════════════════════════════════════════════════════════════

public class UpdateSatelliteUseCaseTests
{
    private readonly Mock<IUnitOfWork>          _uow;
    private readonly Mock<ISatelliteRepository> _satelliteRepo;
    private readonly Mock<IOperatorRepository>  _operatorRepo;
    private readonly UpdateSatelliteUseCase     _sut;

    public UpdateSatelliteUseCaseTests()
    {
        _uow           = new Mock<IUnitOfWork>();
        _satelliteRepo = new Mock<ISatelliteRepository>();
        _operatorRepo  = new Mock<IOperatorRepository>();
        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);
        _uow.Setup(u => u.Operators).Returns(_operatorRepo.Object);
        _sut = new UpdateSatelliteUseCase(_uow.Object);
    }

    private static Satellite MakeSatellite(Guid? operatorId = null)
    {
        var op = new Operator { Id = operatorId ?? Guid.NewGuid(), Name = "TestCorp" };
        var sat = Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
        sat.SetOrbit(new OrbitParameters(6_778_000, 0.001, 51.6, 0, 0, 0));
        return sat;
    }

    private static UpdateSatelliteRequest ValidRequest(Guid? operatorId = null) =>
        new UpdateSatelliteRequest
        {
            Name              = "UpdatedSat",
            LaunchDate        = DateTime.UtcNow.AddYears(-2),
            OperatorId        = operatorId ?? Guid.NewGuid(),
            NoradId           = 12345,
            SemiMajorAxis     = 6_778_000,
            Eccentricity      = 0.001,
            Inclination       = 45.0,
            RAAN              = 0,
            ArgumentOfPerigee = 0,
            MeanAnomaly       = 0
        };

    private void SetupFind(Guid id, Satellite? returns) =>
        _satelliteRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    private void SetupFindOperator(Guid id, Operator? returns) =>
        _operatorRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync(returns);

    [Fact]
    public async Task Execute_WhenSatelliteNotFound_ThrowsException()
    {
        SetupFind(Guid.NewGuid(), null);

        Func<Task> act = () => _sut.Execute(Guid.NewGuid(), ValidRequest(), "admin");

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Execute_WhenSatelliteDeleted_ThrowsException()
    {
        var satellite = MakeSatellite();
        satellite.SoftDelete("admin");
        SetupFind(satellite.Id, satellite);

        Func<Task> act = () => _sut.Execute(satellite.Id, ValidRequest(), "admin");

        await act.Should().ThrowAsync<Exception>().WithMessage("*deleted*");
    }

    [Fact]
    public async Task Execute_WhenOperatorNotFound_ThrowsException()
    {
        var satellite = MakeSatellite();
        SetupFind(satellite.Id, satellite);
        var request = ValidRequest();
        SetupFindOperator(request.OperatorId, null);

        Func<Task> act = () => _sut.Execute(satellite.Id, request, "admin");

        await act.Should().ThrowAsync<Exception>().WithMessage("*Operator not found*");
    }

    [Fact]
    public async Task Execute_WhenGuardFails_DoesNotCommit()
    {
        SetupFind(Guid.NewGuid(), null);

        try { await _sut.Execute(Guid.NewGuid(), ValidRequest(), "admin"); } catch { }

        _uow.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CallsUpdateAsync()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        var request   = ValidRequest(operatorId: opId);
        var op        = new Operator { Id = opId, Name = "TestCorp" };
        SetupFind(satellite.Id, satellite);
        SetupFindOperator(opId, op);
        _satelliteRepo
            .Setup(r => r.UpdateAsync(satellite, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        await _sut.Execute(satellite.Id, request, "admin");

        _satelliteRepo.Verify(
            r => r.UpdateAsync(satellite, It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CommitsUnitOfWork()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        var request   = ValidRequest(operatorId: opId);
        var op        = new Operator { Id = opId, Name = "TestCorp" };
        SetupFind(satellite.Id, satellite);
        SetupFindOperator(opId, op);
        _satelliteRepo
            .Setup(r => r.UpdateAsync(satellite, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        await _sut.Execute(satellite.Id, request, "admin");

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_NullsOperatorNavigationProperty()
    {
        var opId      = Guid.NewGuid();
        var satellite = MakeSatellite(opId);
        var request   = ValidRequest(operatorId: opId);
        var op        = new Operator { Id = opId, Name = "TestCorp" };
        SetupFind(satellite.Id, satellite);
        SetupFindOperator(opId, op);
        _satelliteRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Satellite>(), It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        await _sut.Execute(satellite.Id, request, "admin");

        satellite.Operator.Should().BeNull();
    }
}