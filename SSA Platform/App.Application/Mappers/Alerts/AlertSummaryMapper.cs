using App.Application.Features.Alerts.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Alerts;

public class AlertSummaryMapper : IBaseMapper<AlertSummaryDto, Alert>
{
    public AlertSummaryDto? Map(Alert? entity)
    {
        if (entity is null) return null;

        return new AlertSummaryDto
        {
            Id                  = entity.Id,
            IsAcknowledged      = entity.IsAcknowledged,
            CreatedAt           = entity.CreatedAt,
            Severity            = entity.Severity,
            Message             = entity.Message?.ToString(),
            ConjunctionEventId  = entity.ConjunctionEventId,
            HasConjunctionEvent = entity.ConjunctionEvent != null
        };
    }

    public Alert? Map(AlertSummaryDto? entity)
    {
        if (entity is null) return null;
        
        throw new NotImplementedException();
    }
}