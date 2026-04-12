using App.Application.Features.Alerts.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Alerts.UseCases;

public class GetOperatorAlertsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorAlertsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<AlertSummaryDto>> ExecuteAsync(Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();
        var alerts = await _uow.Alerts.GetByConjunctionIdsAsync(conjunctionIds);

        return alerts.Select(a => new AlertSummaryDto
        {
            Id                  = a.Id,
            IsAcknowledged      = a.IsAcknowledged,
            CreatedAt           = a.CreatedAt,
            Severity            = a.Severity,
            Message             = a.Message?.ToString(),
            ConjunctionEventId  = a.ConjunctionEventId,
            HasConjunctionEvent = a.ConjunctionEvent != null
        });
    }
}
