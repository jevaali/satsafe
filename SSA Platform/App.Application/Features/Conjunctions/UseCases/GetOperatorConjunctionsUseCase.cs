using App.Application.Features.Conjunctions.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Conjunctions.UseCases;

public class GetOperatorConjunctionsUseCase : BaseUseCase<ConjunctionSummaryDto, ConjunctionEvent>
{
    public GetOperatorConjunctionsUseCase(IUnitOfWork uow, IBaseMapper<ConjunctionSummaryDto, ConjunctionEvent> mapper)
        : base(uow, mapper) { }

    public async Task<IEnumerable<ConjunctionSummaryDto>> ExecuteAsync(Guid operatorId)
    {
        var satellites   = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await Uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);

        return conjunctions.Select(Mapper.Map).OfType<ConjunctionSummaryDto>();
    }
}
