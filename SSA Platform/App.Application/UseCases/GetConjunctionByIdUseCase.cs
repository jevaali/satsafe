using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class GetConjunctionByIdUseCase
{
    private readonly IUnitOfWork _uow;

    public GetConjunctionByIdUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ConjunctionEvent?> ExecuteAsync(Guid conjunctionId, Guid operatorId)
    {
        var satellites = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();

        var conjunction = await _uow.ConjunctionEvents.GetByIdAsync(conjunctionId);
        if (conjunction == null ||
            (!satelliteIds.Contains(conjunction.PrimaryObjectId) &&
             !satelliteIds.Contains(conjunction.SecondaryObjectId)))
            return null;

        return conjunction;
    }
}
