using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Services;
using App.Domain.ValueObjects;

namespace App.Application.UseCases;

public class CalculateSatelliteRiskUseCase
{
    private const string ModelVersion = "v1-orbit-overlap";

    private readonly IUnitOfWork _unitOfWork;
    private readonly SatelliteRiskScoringService _scoringService;

    public CalculateSatelliteRiskUseCase(
        IUnitOfWork unitOfWork,
        SatelliteRiskScoringService scoringService)
    {
        _unitOfWork = unitOfWork;
        _scoringService = scoringService;
    }

    /// <summary>
    /// Called at startup. Creates a RiskAssessment for every satellite that
    /// does not yet have one. Satellites that already have a score are skipped.
    /// </summary>
    public async Task CalculateForAllNewAsync()
    {
        var allObjects = (await _unitOfWork.SpaceObjects.GetAllWithOrbitsAsync()).ToList();
        var allOrbits  = allObjects
            .Select(o => o.Orbit?.Parameters)
            .Where(p => p is not null)
            .Cast<OrbitParameters>()
            .ToList();

        var satellites = await _unitOfWork.Satellites.GetAllAsync();

        foreach (var satellite in satellites)
        {
            if (satellite.Orbit?.Parameters is null) continue;

            var existing = await _unitOfWork.RiskAssessments.GetBySatelliteIdAsync(satellite.Id);
            if (existing is not null) continue;

            var otherOrbits = allOrbits
                .Where(p => p != satellite.Orbit.Parameters)
                .ToList();

            var score = _scoringService.Calculate(satellite.Orbit.Parameters, otherOrbits);

            var assessment = new RiskAssessment
            {
                Id             = Guid.NewGuid(),
                RiskScore      = score,
                ModelVersion   = ModelVersion,
                CalculatedAt   = DateTime.UtcNow,
                SatelliteId    = satellite.Id,
                Satellite      = satellite,
                CreatedBy      = "system",
                CreatedAt      = DateTime.UtcNow
            };

            await _unitOfWork.RiskAssessments.AddAsync(assessment);
        }

        await _unitOfWork.CommitAsync();
    }

    /// <summary>
    /// Recalculates the risk score for a specific satellite, creating or
    /// updating its RiskAssessment. Used by the operator regenerate action.
    /// </summary>
    public async Task CalculateForSatelliteAsync(Guid satelliteId)
    {
        var satellite = await _unitOfWork.Satellites.GetByIdAsync(satelliteId);
        if (satellite?.Orbit?.Parameters is null)
            throw new InvalidOperationException("Satellite has no orbit data — cannot calculate risk.");

        var allObjects  = (await _unitOfWork.SpaceObjects.GetAllWithOrbitsAsync()).ToList();
        var otherOrbits = allObjects
            .Where(o => o.Id != satelliteId)
            .Select(o => o.Orbit?.Parameters)
            .Where(p => p is not null)
            .Cast<OrbitParameters>()
            .ToList();

        var score = _scoringService.Calculate(satellite.Orbit.Parameters, otherOrbits);

        var existing = await _unitOfWork.RiskAssessments.GetBySatelliteIdAsync(satelliteId);
        if (existing is not null)
        {
            existing.UpdateScore(score);
            existing.ModelVersion = ModelVersion;
            await _unitOfWork.RiskAssessments.UpdateAsync(existing);
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
                Satellite    = satellite,
                CreatedBy    = "system",
                CreatedAt    = DateTime.UtcNow
            };
            await _unitOfWork.RiskAssessments.AddAsync(assessment);
        }

        await _unitOfWork.CommitAsync();
    }
}
