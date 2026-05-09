using App.Application.Features.Conjunctions.UseCases;
using App.Domain.Entities;
using App.Domain.Services;
using App.Domain.ValueObjects;
using Moq;

namespace App.Tests.Unit.UseCases.Conjunctions;

public class RunConjunctionScreeningUseCaseTests : ConjunctionUseCaseTestBase
{
    private readonly Mock<ConjunctionAnalysisService> _screeningService;
    private readonly RunConjunctionScreeningUseCase   _sut;

    public RunConjunctionScreeningUseCaseTests()
    {
        // ConjunctionAnalysisService is a concrete class with no interface —
        // mark its methods virtual or use the real instance. Here we use real.
        _screeningService = new Mock<ConjunctionAnalysisService>();
        _sut = new RunConjunctionScreeningUseCase(Uow.Object, _screeningService.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFewerThanTwoSatellitesHaveOrbits_Returns()
    {
        var operatorId = Guid.NewGuid();
        var satellite  = MakeSatellite(operatorId); // no orbit
        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { satellite });

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        // Never progresses to fetching space objects
        SpaceObjectsRepo.Verify(
            r => r.GetAllWithOrbitsAsync(),
            Times.Never);
    }

    
    public async Task ExecuteAsync_WhenNoNewEvents_DoesNotCommit()
    {
        var operatorId = Guid.NewGuid();
        var op         = new Operator { Id = operatorId, Name = "TestCorp" };
        var s1         = Satellite.Create("A", DateTime.UtcNow, op);
        var s2         = Satellite.Create("B", DateTime.UtcNow, op);
        s1.SetOrbit(new OrbitParameters(6_778_000, 0.001, 51.6, 0, 0, 0));
        s2.SetOrbit(new OrbitParameters(6_778_000, 0.001, 51.6, 0, 0, 0));

        SatelliteRepo
            .Setup(r => r.GetByOperatorIdAsync(operatorId))
            .ReturnsAsync(new List<Satellite> { s1, s2 });
        NotificationRepo
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((NotificationSetting?)null);
        SpaceObjectsRepo
            .Setup(r => r.GetAllWithOrbitsAsync())
            .ReturnsAsync(new List<SpaceObject>());
        _screeningService
            .Setup(s => s.ScreenAll(
                It.IsAny<IReadOnlyList<SpaceObject>>(),
                It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
            .Returns(new List<ConjunctionEvent>());

        await _sut.ExecuteAsync(operatorId, Guid.NewGuid());

        Uow.Verify(u => u.CommitAsync(), Times.Never);
    }
}