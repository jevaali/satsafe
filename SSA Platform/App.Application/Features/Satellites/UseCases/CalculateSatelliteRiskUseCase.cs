using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Services;
using App.Domain.ValueObjects;

namespace App.Application.Features.Satellites.UseCases;

public class CalculateSatelliteRiskUseCase : BaseUseCase
{
    private const string ModelVersion = "v1-orbit-overlap";

    private readonly SatelliteRiskScoringService _scoringService;

    public CalculateSatelliteRiskUseCase(IUnitOfWork uow, SatelliteRiskScoringService scoringService) : base(uow)
    {
        _scoringService = scoringService;
    }

    public async Task CalculateForAllNewAsync()
    {
        var allObjects = (await Uow.SpaceObjects.GetAllWithOrbitsAsync()).ToList();
        var allOrbits  = allObjects
            .Select(o => o.Orbit?.Parameters)
            .Where(p => p is not null)
            .Cast<OrbitParameters>()
            .ToList();

        var satellites = await Uow.Satellites.AllAsync();

        foreach (var satellite in satellites)
        {
            if (satellite.Orbit?.Parameters is null) continue;

            var existing = await Uow.RiskAssessments.GetBySatelliteIdAsync(satellite.Id);
            if (existing is not null) continue;

            var otherOrbits = allOrbits
                .Where(p => p != satellite.Orbit.Parameters)
                .ToList();

            var score = _scoringService.Calculate(satellite.Orbit.Parameters, otherOrbits);

            var assessment = new RiskAssessment
            {
                Id           = Guid.NewGuid(),
                RiskScore    = score,
                ModelVersion = ModelVersion,
                CalculatedAt = DateTime.UtcNow,
                SatelliteId  = satellite.Id,
                Satellite    = null!,
                CreatedBy    = "system",
                CreatedAt    = DateTime.UtcNow
            };

            Uow.RiskAssessments.Add(assessment);
        }

        await Uow.CommitAsync();
    }

    public async Task CalculateForSatelliteAsync(Guid satelliteId)
    {
        var satellite = await Uow.Satellites.FindAsync(satelliteId);
        if (satellite?.Orbit?.Parameters is null)
            throw new InvalidOperationException("Satellite has no orbit data — cannot calculate risk.");

        var allObjects  = (await Uow.SpaceObjects.GetAllWithOrbitsAsync()).ToList();
        var otherOrbits = allObjects
            .Where(o => o.Id != satelliteId)
            .Select(o => o.Orbit?.Parameters)
            .Where(p => p is not null)
            .Cast<OrbitParameters>()
            .ToList();

        var score = _scoringService.Calculate(satellite.Orbit.Parameters, otherOrbits);

        var existing = await Uow.RiskAssessments.GetBySatelliteIdAsync(satelliteId);
        if (existing is not null)
        {
            existing.UpdateScore(score);
            existing.ModelVersion = ModelVersion;
            await Uow.RiskAssessments.UpdateAsync(existing);
        }
        else
        {
            var assessment = new RiskAssessment
            {
                Id           = Guid.NewGuid(),
                RiskScore    = score,
                ModelVersion = ModelVersion,
                CalculatedAt = DateTime.UtcNow,
                SatelliteId  = satelliteId,
                Satellite    = null!,
                CreatedBy    = "system",
                CreatedAt    = DateTime.UtcNow
            };
            Uow.RiskAssessments.Add(assessment);
        }

        await Uow.CommitAsync();
    }
}
