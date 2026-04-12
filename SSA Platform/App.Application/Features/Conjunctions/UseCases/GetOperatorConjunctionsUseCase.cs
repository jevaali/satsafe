using App.Application.Features.Conjunctions.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Conjunctions.UseCases;

public class GetOperatorConjunctionsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorConjunctionsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<ConjunctionSummaryDto>> ExecuteAsync(Guid operatorId)
    {
        var satellites   = await _uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await _uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);

        return conjunctions.Select(MapToDto);
    }

    private static ConjunctionSummaryDto MapToDto(ConjunctionEvent c) => new()
    {
        Id                    = c.Id,
        IsHighRisk            = c.IsHighRisk(),
        TimeOfClosestApproach = c.TimeOfClosestApproach,
        PrimaryObjectId       = c.PrimaryObjectId,
        SecondaryObjectId     = c.SecondaryObjectId,
        PrimaryObjectName     = c.PrimaryObject switch
        {
            Satellite s    => s.Name.ToString(),
            DebrisObject d => $"DEBRIS-{d.NoradId}",
            _              => c.PrimaryObjectId.ToString()
        },
        SecondaryObjectName = c.SecondaryObject switch
        {
            Satellite s    => s.Name.ToString(),
            DebrisObject d => $"DEBRIS-{d.NoradId}",
            _              => c.SecondaryObjectId.ToString()
        },
        MissDistance         = c.MissDistance,
        CollisionProbability = c.CollisionProbability,
        Status               = c.Status
    };
}
