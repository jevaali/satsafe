using App.Application.Common.DTOs;
using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Conjunctions.DTOs;
using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Dashboard.UseCases;

public class GetOperatorDashboardUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOperatorDashboardUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperatorDashboardDto> ExecuteAsync(Guid userId, Guid operatorId)
    {
        var satellites  = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();

        var conjunctions  = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alerts = await _unitOfWork.Alerts.GetByConjunctionIdsAsync(conjunctionIds);

        var operatorEntity = await _unitOfWork.Operators.GetByIdAsync(operatorId);

        return new OperatorDashboardDto
        {
            OperatorName         = operatorEntity?.Name?.ToString() ?? "Operator",
            TotalSatellites      = satellites.Count(),
            ActiveSatellites     = satellites.Count(s => s.Status == ESatelliteStatus.Active),
            PendingConjunctions  = conjunctions.Count(c => c.Status == EConjunctionEventStatus.Pending),
            HighRiskConjunctions = conjunctions.Count(c => c.IsHighRisk()),
            UnacknowledgedAlerts = alerts.Count(a => !a.IsAcknowledged),
            Satellites = satellites.Select(s => new SatelliteSummaryDto
            {
                Id        = s.Id,
                Name      = s.Name.ToString(),
                NoradId   = s.NoradId,
                LaunchDate = s.LaunchDate,
                Status    = s.Status,
                RiskLevel = s.RiskAssessment?.RiskLevel,
                Orbit     = s.Orbit == null ? null : new OrbitDto
                {
                    SemiMajorAxis     = s.Orbit.Parameters.SemiMajorAxis,
                    Eccentricity      = s.Orbit.Parameters.Eccentricity,
                    Inclination       = s.Orbit.Parameters.Inclination,
                    RAAN              = s.Orbit.Parameters.RAAN,
                    ArgumentOfPerigee = s.Orbit.Parameters.ArgumentOfPerigee,
                    MeanAnomaly       = s.Orbit.Parameters.MeanAnomaly
                }
            }).ToList(),
            Conjunctions = conjunctions.Select(c => new ConjunctionSummaryDto
            {
                Id                    = c.Id,
                IsHighRisk            = c.IsHighRisk(),
                TimeOfClosestApproach = c.TimeOfClosestApproach,
                PrimaryObjectId       = c.PrimaryObjectId,
                SecondaryObjectId     = c.SecondaryObjectId,
                PrimaryObjectName     = c.PrimaryObject switch
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
                MissDistance         = c.MissDistance,
                CollisionProbability = c.CollisionProbability
            }).ToList(),
            Alerts = alerts.Select(a => new AlertSummaryDto
            {
                Id             = a.Id,
                IsAcknowledged = a.IsAcknowledged,
                CreatedAt      = a.CreatedAt,
                Severity       = a.Severity,
                Message        = a.Message?.ToString()
            }).ToList()
        };
    }
}
