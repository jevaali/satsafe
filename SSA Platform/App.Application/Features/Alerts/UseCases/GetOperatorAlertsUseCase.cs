using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Alerts.UseCases;

public class GetOperatorAlertsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorAlertsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Alert>> ExecuteAsync(Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();
        return await _uow.Alerts.GetByConjunctionIdsAsync(conjunctionIds);
    }
}
