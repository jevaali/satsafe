using App.Application.Features.Alerts.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Alerts.UseCases;

public class GetAlertByIdUseCase
{
    private readonly IUnitOfWork _uow;

    public GetAlertByIdUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AlertDetailsDto?> ExecuteAsync(Guid alertId, Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alert = await _uow.Alerts.GetByIdAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            return null;

        var c = alert.ConjunctionEvent;
        return new AlertDetailsDto
        {
            Id                 = alert.Id,
            IsAcknowledged     = alert.IsAcknowledged,
            CreatedAt          = alert.CreatedAt,
            UpdatedAt          = alert.UpdatedAt,
            Severity           = alert.Severity,
            Message            = alert.Message?.ToString(),
            ConjunctionEventId = alert.ConjunctionEventId,
            ConjunctionEvent   = c == null ? null : new ConjunctionDetailsDto
            {
                Id                    = c.Id,
                TimeOfClosestApproach = c.TimeOfClosestApproach,
                MissDistance          = c.MissDistance,
                CollisionProbability  = c.CollisionProbability,
                IsHighRisk            = c.IsHighRisk(),
                Status                = c.Status
            }
        };
    }
}
