using App.Application.Features.Conjunctions.UseCases;
using App.Application.Mappers.Alerts;
using App.Application.Mappers.Conjunctions;
using App.Application.Mappers.Shared;
using App.Domain.Entities;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public class GetConjunctionByIdUseCaseTests : ConjunctionUseCaseTestBase
{
    private readonly GetConjunctionByIdUseCase _sut;

    public GetConjunctionByIdUseCaseTests()
    {
        var orbitMapper  = new OrbitMapper();
        var alertMapper  = new AlertSummaryMapper();
        var mapper       = new ConjunctionDetailMapper(alertMapper, orbitMapper);
        _sut = new GetConjunctionByIdUseCase(Uow.Object, mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WhenConjunctionNotFound_ReturnsNull()
    {
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((ConjunctionEvent?)null);

        var result = await _sut.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsNeitherObject_ReturnsNull()
    {
        var operatorId  = Guid.NewGuid();
        var conjunction = MakeConjunction(Guid.NewGuid(), Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.FindAsync(conjunction.Id, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        var result = await _sut.ExecuteAsync(conjunction.Id, operatorId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsPrimary_ReturnsDtoWithOperatorSidePrimary()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.FindAsync(conjunction.Id, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        var result = await _sut.ExecuteAsync(conjunction.Id, operatorId);

        result.Should().NotBeNull();
        result!.OperatorSide.Should().Be("Primary");
    }

    [Fact]
    public async Task ExecuteAsync_WhenOperatorOwnsSecondary_ReturnsDtoWithOperatorSideSecondary()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(Guid.NewGuid(), satellite.Id);
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.FindAsync(conjunction.Id, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        var result = await _sut.ExecuteAsync(conjunction.Id, operatorId);

        result!.OperatorSide.Should().Be("Secondary");
    }

    [Fact]
    public async Task ExecuteAsync_WhenFound_MapsMissDistance()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid(), missDistance: 3_000);
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.FindAsync(conjunction.Id, It.IsAny<Guid>()))
            .ReturnsAsync(conjunction);

        var result = await _sut.ExecuteAsync(conjunction.Id, operatorId);

        result!.MissDistance.Should().Be(3_000);
    }
}