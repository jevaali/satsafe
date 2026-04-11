using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class GetOperatorConjunctionsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorConjunctionsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ConjunctionEvent>> ExecuteAsync(Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        return await _uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
    }
}
