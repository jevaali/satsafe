using App.Application.Features.Conjunctions.UseCases;
using App.Application.Mappers.Conjunctions;
using App.Domain.Entities;
using AwesomeAssertions;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public class GetOperatorConjunctionsUseCaseTests : ConjunctionUseCaseTestBase
{
    private readonly GetOperatorConjunctionsUseCase _sut;

    public GetOperatorConjunctionsUseCaseTests()
    {
        var mapper = new ConjunctionSummaryMapper();
        _sut = new GetOperatorConjunctionsUseCase(Uow.Object, mapper);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoSatellites_ReturnsEmptyList()
    {
        var operatorId = Guid.NewGuid();
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite>());
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent>());

        var result = await _sut.ExecuteAsync(operatorId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsDtoForEachConjunction()
    {
        var operatorId   = Guid.NewGuid();
        var satellite    = MakeSatellite(operatorId);
        var conjunctions = new List<ConjunctionEvent>
        {
            MakeConjunction(satellite.Id, Guid.NewGuid()),
            MakeConjunction(satellite.Id, Guid.NewGuid())
        };

        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(conjunctions);

        var result = await _sut.ExecuteAsync(operatorId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_MapsConjunctionIdToDto()
    {
        var operatorId  = Guid.NewGuid();
        var satellite   = MakeSatellite(operatorId);
        var conjunction = MakeConjunction(satellite.Id, Guid.NewGuid());

        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });
        ConjunctionRepo
            .Setup(r => r.GetBySatelliteIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<ConjunctionEvent> { conjunction });

        var result = await _sut.ExecuteAsync(operatorId);

        result.Single().Id.Should().Be(conjunction.Id);
    }
}