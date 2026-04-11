using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class GetAlertByIdUseCase
{
    private readonly IUnitOfWork _uow;

    public GetAlertByIdUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Alert?> ExecuteAsync(Guid alertId, Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alert = await _uow.Alerts.GetByIdAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            return null;

        return alert;
    }
}
