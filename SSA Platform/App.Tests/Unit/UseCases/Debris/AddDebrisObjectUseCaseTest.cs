using App.Application.Features.Debris.DTOs;
using App.Application.Features.Debris.UseCases;
using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Debris;

public class AddDebrisObjectUseCaseTests
{
    // ── Dependencies ──────────────────────────────────────────────────────────

    private readonly Mock<IUnitOfWork>               _uow;
    private readonly Mock<IDebrisObjectRepository>   _debrisRepo;
    private readonly AddDebrisObjectUseCase          _sut;

    public AddDebrisObjectUseCaseTests()
    {
        _uow        = new Mock<IUnitOfWork>();
        _debrisRepo = new Mock<IDebrisObjectRepository>();
        _uow.Setup(u => u.DebrisObjects).Returns(_debrisRepo.Object);
        _sut = new AddDebrisObjectUseCase(_uow.Object);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AddDebrisObjectRequest ValidRequest(
        EDebrisObjectRiskCategory riskCategory = EDebrisObjectRiskCategory.Medium) =>
        new AddDebrisObjectRequest
        {
            SizeEstimate      = 2.5f,
            RiskCategory      = riskCategory,
            SemiMajorAxis     = 6_778_000,
            Eccentricity      = 0.001,
            Inclination       = 51.6,
            RAAN              = 0,
            ArgumentOfPerigee = 0,
            MeanAnomaly       = 0
        };

    // ── Return value ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Execute_WithValidRequest_ReturnsNonEmptyGuid()
    {
        var id = await _sut.Execute(ValidRequest());

        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Execute_CalledTwice_ReturnsDifferentIds()
    {
        var id1 = await _sut.Execute(ValidRequest());
        var id2 = await _sut.Execute(ValidRequest());

        id1.Should().NotBe(id2);
    }

    // ── Repository interactions ───────────────────────────────────────────────

    [Fact]
    public async Task Execute_WithValidRequest_AddsDebrisObjectToRepository()
    {
        await _sut.Execute(ValidRequest());

        _debrisRepo.Verify(
            r => r.Add(It.IsAny<DebrisObject>(), It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_WithValidRequest_CommitsUnitOfWork()
    {
        await _sut.Execute(ValidRequest());

        _uow.Verify(u => u.CommitAsync(), Times.Once);
    }

    // ── Debris object built correctly ─────────────────────────────────────────

    [Fact]
    public async Task Execute_WithValidRequest_DebrisObjectHasOrbitSet()
    {
        DebrisObject? added = null;
        _debrisRepo
            .Setup(r => r.Add(It.IsAny<DebrisObject>(), It.IsAny<Guid>()))
            .Callback<DebrisObject, Guid>((d, _) => added = d);

        await _sut.Execute(ValidRequest());

        added!.Orbit.Should().NotBeNull();
    }

    [Fact]
    public async Task Execute_WithValidRequest_DebrisObjectOrbitMatchesRequest()
    {
        var request = ValidRequest();
        DebrisObject? added = null;
        _debrisRepo
            .Setup(r => r.Add(It.IsAny<DebrisObject>(), It.IsAny<Guid>()))
            .Callback<DebrisObject, Guid>((d, _) => added = d);

        await _sut.Execute(request);

        added!.Orbit!.Parameters.Inclination.Should().Be(request.Inclination);
        added.Orbit.Parameters.SemiMajorAxis.Should().Be(request.SemiMajorAxis);
    }

    [Fact]
    public async Task Execute_WithValidRequest_DebrisObjectHasCorrectSizeEstimate()
    {
        var request = ValidRequest();
        DebrisObject? added = null;
        _debrisRepo
            .Setup(r => r.Add(It.IsAny<DebrisObject>(), It.IsAny<Guid>()))
            .Callback<DebrisObject, Guid>((d, _) => added = d);

        await _sut.Execute(request);

        added!.SizeEstimate.Should().Be(request.SizeEstimate);
    }

    // ── Theory: all risk categories pass through correctly ────────────────────

    [Theory]
    [InlineData(EDebrisObjectRiskCategory.Low)]
    [InlineData(EDebrisObjectRiskCategory.Medium)]
    [InlineData(EDebrisObjectRiskCategory.High)]
    public async Task Execute_WithAnyRiskCategory_DebrisObjectHasCorrectRiskCategory(
        EDebrisObjectRiskCategory riskCategory)
    {
        var request = ValidRequest(riskCategory);
        DebrisObject? added = null;
        _debrisRepo
            .Setup(r => r.Add(It.IsAny<DebrisObject>(), It.IsAny<Guid>()))
            .Callback<DebrisObject, Guid>((d, _) => added = d);

        await _sut.Execute(request);

        added!.RiskCategory.Should().Be(riskCategory);
    }
}