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
        // Validation checks to ensure the user has access to the operator's dashboard
        
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            throw new ArgumentException("Invalid user or operator ID.");
        
        if (await _unitOfWork.UserOperators.GetByUserAndOperatorAsync(userId, operatorId) == null)
            throw new UnauthorizedAccessException("User does not have access to this operator's dashboard.");
        
        // Fetch satellites, conjunctions, and alerts for the operator
        
        var satellites  = await _unitOfWork.Satellites.GetByOperatorIdAsync(operatorId);
        var enumerable = satellites.ToList();
        var satelliteIds = enumerable.Select(s => s.Id).ToList();

        var conjunctions  = await _unitOfWork.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionEvents = conjunctions as ConjunctionEvent[] ?? conjunctions.ToArray();
        var conjunctionIds = conjunctionEvents.Select(c => c.Id).ToList();

        var alerts = await _unitOfWork.Alerts.GetByConjunctionIdsAsync(conjunctionIds);

        var operatorEntity = await _unitOfWork.Operators.GetByIdAsync(operatorId);

        var enumerable1 = alerts as Alert[] ?? alerts.ToArray();
        return new OperatorDashboardDto
        {
            OperatorName         = operatorEntity?.Name.ToString() ?? "Operator",
            TotalSatellites      = enumerable.Count(),
            ActiveSatellites     = enumerable.Count(s => s.Status == ESatelliteStatus.Active),
            PendingConjunctions  = conjunctionEvents.Count(c => c.Status == EConjunctionEventStatus.Pending),
            HighRiskConjunctions = conjunctionEvents.Count(c => c.IsHighRisk()),
            UnacknowledgedAlerts = enumerable1.Count(a => !a.IsAcknowledged),
            Satellites = enumerable.Select(s => new SatelliteSummaryDto
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
            Conjunctions = conjunctionEvents.Select(c => new ConjunctionSummaryDto
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
            Alerts = enumerable1.Select(a => new AlertSummaryDto
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
