using App.Application.Common.DTOs;
using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Conjunctions.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Conjunctions.UseCases;

public class GetConjunctionByIdUseCase
{
    private readonly IUnitOfWork _uow;

    public GetConjunctionByIdUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ConjunctionDetailDto?> ExecuteAsync(Guid conjunctionId, Guid operatorId)
    {
        var satellites   = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToHashSet();

        var conjunction = await _uow.ConjunctionEvents.GetByIdAsync(conjunctionId);
        if (conjunction == null ||
            (!satelliteIds.Contains(conjunction.PrimaryObjectId) &&
             !satelliteIds.Contains(conjunction.SecondaryObjectId)))
            return null;

        var operatorSide = satelliteIds.Contains(conjunction.PrimaryObjectId)   ? "Primary"
                         : satelliteIds.Contains(conjunction.SecondaryObjectId) ? "Secondary"
                         : "None";

        return MapToDto(conjunction, operatorSide);
    }

    private static ConjunctionDetailDto MapToDto(ConjunctionEvent c, string operatorSide) => new()
    {
        Id                    = c.Id,
        IsHighRisk            = c.IsHighRisk(),
        TimeOfClosestApproach = c.TimeOfClosestApproach,
        PrimaryObjectId       = c.PrimaryObjectId,
        SecondaryObjectId     = c.SecondaryObjectId,
        MissDistance          = c.MissDistance,
        CollisionProbability  = c.CollisionProbability,
        Status                = c.Status,
        PrimaryAcknowledgement   = c.PrimaryAcknowledgement,
        SecondaryAcknowledgement = c.SecondaryAcknowledgement,
        OperatorSide          = operatorSide,

        PrimaryObjectName = c.PrimaryObject switch
        {
            Satellite s    => s.Name.ToString(),
            DebrisObject d => $"DEBRIS-{d.NoradId}",
            _              => c.PrimaryObjectId.ToString()
        },
        SecondaryObjectName = c.SecondaryObject switch
        {
            Satellite s    => s.Name.ToString(),
            DebrisObject d => $"DEBRIS-{d.NoradId}",
            _              => c.SecondaryObjectId.ToString()
        },

        PrimaryIsSatellite   = c.PrimaryObject   is Satellite,
        SecondaryIsSatellite = c.SecondaryObject is Satellite,

        PrimaryNoradId = c.PrimaryObject switch
        {
            Satellite s    => s.NoradId,
            DebrisObject d => d.NoradId,
            _              => null
        },
        SecondaryNoradId = c.SecondaryObject switch
        {
            Satellite s    => s.NoradId,
            DebrisObject d => d.NoradId,
            _              => null
        },

        PrimaryObjectStatus   = (c.PrimaryObject   as Satellite)?.Status,
        SecondaryObjectStatus = (c.SecondaryObject as Satellite)?.Status,

        PrimaryOrbit = c.PrimaryObject?.Orbit == null ? null : new OrbitDto
        {
            SemiMajorAxis     = c.PrimaryObject.Orbit.Parameters.SemiMajorAxis,
            Eccentricity      = c.PrimaryObject.Orbit.Parameters.Eccentricity,
            Inclination       = c.PrimaryObject.Orbit.Parameters.Inclination,
            RAAN              = c.PrimaryObject.Orbit.Parameters.RAAN,
            ArgumentOfPerigee = c.PrimaryObject.Orbit.Parameters.ArgumentOfPerigee,
            MeanAnomaly       = c.PrimaryObject.Orbit.Parameters.MeanAnomaly
        },
        SecondaryOrbit = c.SecondaryObject?.Orbit == null ? null : new OrbitDto
        {
            SemiMajorAxis     = c.SecondaryObject.Orbit.Parameters.SemiMajorAxis,
            Eccentricity      = c.SecondaryObject.Orbit.Parameters.Eccentricity,
            Inclination       = c.SecondaryObject.Orbit.Parameters.Inclination,
            RAAN              = c.SecondaryObject.Orbit.Parameters.RAAN,
            ArgumentOfPerigee = c.SecondaryObject.Orbit.Parameters.ArgumentOfPerigee,
            MeanAnomaly       = c.SecondaryObject.Orbit.Parameters.MeanAnomaly
        },

        Alerts = c.Alerts
            .OrderByDescending(a => !a.IsAcknowledged)
            .ThenByDescending(a => a.Severity)
            .Select(a => new AlertSummaryDto
            {
                Id               = a.Id,
                IsAcknowledged   = a.IsAcknowledged,
                CreatedAt        = a.CreatedAt,
                Severity         = a.Severity,
                Message          = a.Message?.ToString(),
                ConjunctionEventId  = a.ConjunctionEventId,
                HasConjunctionEvent = true
            })
            .ToList()
    };
}
