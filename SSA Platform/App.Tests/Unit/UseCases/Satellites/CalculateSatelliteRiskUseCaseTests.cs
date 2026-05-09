using App.Application.Features.Satellites.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.Services;
using App.Domain.ValueObjects;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Satellites;

public class CalculateSatelliteRiskUseCaseTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork>               _uow;
    private readonly Mock<ISatelliteRepository>      _satelliteRepo;
    private readonly Mock<ISpaceObjectsRepository>   _spaceObjectsRepo;
    private readonly Mock<IRiskAssessmentRepository> _riskRepo;
    private readonly SatelliteRiskScoringService     _scoringService;
    private readonly CalculateSatelliteRiskUseCase   _sut;

    public CalculateSatelliteRiskUseCaseTests()
    {
        _uow              = new Mock<IUnitOfWork>();
        _satelliteRepo    = new Mock<ISatelliteRepository>();
        _spaceObjectsRepo = new Mock<ISpaceObjectsRepository>();
        _riskRepo         = new Mock<IRiskAssessmentRepository>();
        _scoringService   = new SatelliteRiskScoringService(); // real — no side effects

        _uow.Setup(u => u.Satellites).Returns(_satelliteRepo.Object);
        _uow.Setup(u => u.SpaceObjects).Returns(_spaceObjectsRepo.Object);
        _uow.Setup(u => u.RiskAssessments).Returns(_riskRepo.Object);

        _sut = new CalculateSatelliteRiskUseCase(_uow.Object, _scoringService);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static OrbitParameters MakeOrbit(double inclination = 51.6) =>
        new OrbitParameters(
            semiMajorAxis:     6_778_000,
            eccentricity:      0.001,
            inclination:       inclination,
            RAAN:              0,
            argumentOfPerigee: 0,
            meanAnomaly:       0);

    // Satellite has a private constructor — use factory methods.
    private static Satellite MakeSatellite(OrbitParameters? orbit = null)
    {
        var op  = new Operator { Id = Guid.NewGuid(), Name = "TestCorp" };
        var sat = Satellite.Create("TestSat", DateTime.UtcNow.AddYears(-1), op);
        if (orbit is not null)
            sat.SetOrbit(orbit);
        return sat;
    }

    // DebrisObject uses a static factory too — no subclassing needed.
    private static DebrisObject MakeDebris(OrbitParameters? orbit = null)
    {
        var d = DebrisObject.Create(1.0f, EDebrisObjectRiskCategory.Low);
        if (orbit is not null)
            d.SetOrbit(orbit);
        return d;
    }

    private static RiskAssessment MakeAssessment(Guid satelliteId, double score = 0.1) =>
        new RiskAssessment
        {
            Id           = Guid.NewGuid(),
            RiskScore    = score,
            ModelVersion = "old",
            CalculatedAt = DateTime.UtcNow.AddDays(-1),
            SatelliteId  = satelliteId,
            Satellite    = null!,
            CreatedBy    = "system",
            CreatedAt    = DateTime.UtcNow.AddDays(-1)
        };

    // ── CalculateForSatelliteAsync ────────────────────────────────────────────

    [Fact]
    public async Task CalculateForSatelliteAsync_WhenSatelliteNotFound_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        _satelliteRepo
            .Setup(r => r.FindAsync(id, It.IsAny<Guid>()))
            .ReturnsAsync((Satellite?)null);

        Func<Task> act = () => _sut.CalculateForSatelliteAsync(id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CalculateForSatelliteAsync_WhenSatelliteHasNoOrbit_ThrowsInvalidOperationException()
    {
        var satellite = MakeSatellite(orbit: null);
        _satelliteRepo
            .Setup(r => r.FindAsync(satellite.Id, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);

        Func<Task> act = () => _sut.CalculateForSatelliteAsync(satellite.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no orbit data*");
    }

    [Fact]
    public async Task CalculateForSatelliteAsync_WhenNoExistingAssessment_AddsNewRiskAssessment()
    {
        var satellite = MakeSatellite(MakeOrbit());
        _satelliteRepo
            .Setup(r => r.FindAsync(satellite.Id, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);
        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync((RiskAssessment?)null);

        await _sut.CalculateForSatelliteAsync(satellite.Id);

        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task CalculateForSatelliteAsync_WhenExistingAssessment_UpdatesInsteadOfAdding()
    {
        var satellite = MakeSatellite(MakeOrbit());
        var existing  = MakeAssessment(satellite.Id);

        _satelliteRepo
            .Setup(r => r.FindAsync(satellite.Id, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);
        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync(existing);
        _riskRepo
            .Setup(r => r.UpdateAsync(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()))
            .ReturnsAsync(existing);

        await _sut.CalculateForSatelliteAsync(satellite.Id);

        _riskRepo.Verify(
            r => r.UpdateAsync(It.Is<RiskAssessment>(a => a.Id == existing.Id), It.IsAny<Guid>()),
            Times.Once);
        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateForSatelliteAsync_WhenExistingAssessment_UpdatesModelVersion()
    {
        var satellite = MakeSatellite(MakeOrbit());
        var existing  = MakeAssessment(satellite.Id);

        _satelliteRepo
            .Setup(r => r.FindAsync(satellite.Id, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);
        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync(existing);
        _riskRepo
            .Setup(r => r.UpdateAsync(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()))
            .ReturnsAsync(existing);

        await _sut.CalculateForSatelliteAsync(satellite.Id);

        existing.ModelVersion.Should().Be("v1-orbit-overlap");
    }

    [Fact]
    public async Task CalculateForSatelliteAsync_Always_CommitsUnitOfWork()
    {
        var satellite = MakeSatellite(MakeOrbit());
        _satelliteRepo
            .Setup(r => r.FindAsync(satellite.Id, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);
        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync((RiskAssessment?)null);

        await _sut.CalculateForSatelliteAsync(satellite.Id);

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateForSatelliteAsync_WhenOnlyItselfInCatalogue_ScoreIsZero()
    {
        // The satellite's own orbit must not be counted as its own neighbour.
        // With only itself in SpaceObjects the other-orbits list is empty → score = 0.
        var satellite = MakeSatellite(MakeOrbit());

        _satelliteRepo
            .Setup(r => r.FindAsync(satellite.Id, It.IsAny<Guid>()))
            .ReturnsAsync(satellite);
        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject> { satellite });
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync((RiskAssessment?)null);

        RiskAssessment? added = null;
        _riskRepo
            .Setup(r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()))
            .Callback<RiskAssessment, Guid>((a, _) => added = a);

        await _sut.CalculateForSatelliteAsync(satellite.Id);

        added!.RiskScore.Should().Be(0.0);
    }

    // ── CalculateForAllNewAsync ───────────────────────────────────────────────

    [Fact]
    public async Task CalculateForAllNewAsync_WhenNoSatellites_CommitsWithoutAddingAnyAssessment()
    {
        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _satelliteRepo
            .Setup(r => r.AllAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite>());

        await _sut.CalculateForAllNewAsync();

        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Never);
        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task CalculateForAllNewAsync_WhenSatelliteAlreadyHasAssessment_SkipsIt()
    {
        var satellite = MakeSatellite(MakeOrbit());
        var existing  = MakeAssessment(satellite.Id);

        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _satelliteRepo
            .Setup(r => r.AllAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite> { satellite });
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync(existing);

        await _sut.CalculateForAllNewAsync();

        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateForAllNewAsync_WhenSatelliteHasNoOrbit_SkipsIt()
    {
        var satellite = MakeSatellite(orbit: null);

        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _satelliteRepo
            .Setup(r => r.AllAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite> { satellite });

        await _sut.CalculateForAllNewAsync();

        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task CalculateForAllNewAsync_WhenOneNewSatelliteWithOrbit_AddsOneAssessment()
    {
        var satellite = MakeSatellite(MakeOrbit());

        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _satelliteRepo
            .Setup(r => r.AllAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite> { satellite });
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(satellite.Id))
            .ReturnsAsync((RiskAssessment?)null);

        await _sut.CalculateForAllNewAsync();

        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task CalculateForAllNewAsync_WithTwoNewSatellites_AddsTwoAssessments()
    {
        var s1 = MakeSatellite(MakeOrbit(inclination: 51.6));
        var s2 = MakeSatellite(MakeOrbit(inclination: 45.0));

        _spaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _satelliteRepo
            .Setup(r => r.AllAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite> { s1, s2 });
        _riskRepo
            .Setup(r => r.GetBySatelliteIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((RiskAssessment?)null);

        await _sut.CalculateForAllNewAsync();

        _riskRepo.Verify(
            r => r.Add(It.IsAny<RiskAssessment>(), It.IsAny<Guid>()),
            Times.Exactly(2));
    }
}