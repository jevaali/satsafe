using App.Application.Features.Alerts.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Alerts;

public class AlertDetailsMapper : IBaseMapper<AlertDetailsDto, Alert>
{
    public AlertDetailsDto? Map(Alert? alert)
    {
        if (alert == null) return null;

        return new AlertDetailsDto
        {
            Id                 = alert.Id,
            IsAcknowledged     = alert.IsAcknowledged,
            CreatedAt          = alert.CreatedAt,
            Severity           = alert.Severity,
            Message            = alert.Message?.ToString(),
            ConjunctionEventId = alert.ConjunctionEventId,
            ConjunctionEvent   = alert.ConjunctionEvent == null ? null : new ConjunctionDetailsDto
            {
                Id                    = alert.ConjunctionEvent.Id,
                TimeOfClosestApproach = alert.ConjunctionEvent.TimeOfClosestApproach,
                MissDistance          = alert.ConjunctionEvent.MissDistance,
                CollisionProbability  = alert.ConjunctionEvent.CollisionProbability,
                IsHighRisk            = alert.ConjunctionEvent.IsHighRisk(),
                Status                = alert.ConjunctionEvent.Status
            }
        };
    }

    public Alert? Map(AlertDetailsDto? dto) => throw new NotImplementedException();
}