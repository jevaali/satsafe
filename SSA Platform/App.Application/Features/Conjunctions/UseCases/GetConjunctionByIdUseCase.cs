using App.Application.Features.Conjunctions.DTOs;
using App.Application.Interfaces;
using App.Application.Mappers.Conjunctions;

namespace App.Application.Features.Conjunctions.UseCases;

public class GetConjunctionByIdUseCase : BaseUseCase
{
    private readonly ConjunctionDetailMapper _mapper;

    public GetConjunctionByIdUseCase(IUnitOfWork uow, ConjunctionDetailMapper mapper) : base(uow)
    {
        _mapper = mapper;
    }

    public async Task<ConjunctionDetailDto?> ExecuteAsync(Guid conjunctionId, Guid operatorId)
    {
        var satellites   = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToHashSet();

        var conjunction = await Uow.ConjunctionEvents.FindAsync(conjunctionId);
        if (conjunction is null ||
            (!satelliteIds.Contains(conjunction.PrimaryObjectId) &&
             !satelliteIds.Contains(conjunction.SecondaryObjectId)))
            return null;

        var operatorSide = satelliteIds.Contains(conjunction.PrimaryObjectId) ? "Primary" : "Secondary";

        return _mapper.Map(conjunction, operatorSide);
    }
}
