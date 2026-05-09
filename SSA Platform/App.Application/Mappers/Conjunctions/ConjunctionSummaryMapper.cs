using App.Application.Features.Conjunctions.DTOs;
using App.Application.Mappers.Shared;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Conjunctions;

public class ConjunctionSummaryMapper : IBaseMapper<ConjunctionSummaryDto, ConjunctionEvent>
{
    public ConjunctionSummaryDto? Map(ConjunctionEvent? c)
    {
        if (c is null) return null;

        return new ConjunctionSummaryDto
        {
            Id                    = c.Id,
            IsHighRisk            = c.IsHighRisk(),
            TimeOfClosestApproach = c.TimeOfClosestApproach,
            PrimaryObjectId       = c.PrimaryObjectId,
            SecondaryObjectId     = c.SecondaryObjectId,
            MissDistance          = c.MissDistance,
            CollisionProbability  = c.CollisionProbability,
            Status                = c.Status,
            PrimaryObjectName     = SpaceObjectResolver.ResolveName(c.PrimaryObject,  c.PrimaryObjectId),
            SecondaryObjectName   = SpaceObjectResolver.ResolveName(c.SecondaryObject, c.SecondaryObjectId),
        };
    }

    public ConjunctionEvent? Map(ConjunctionSummaryDto? entity) => throw new NotImplementedException();


}