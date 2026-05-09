using App.Application.Common.DTOs;
using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Conjunctions.DTOs;
using App.Application.Mappers.Shared;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Conjunctions;

public class ConjunctionDetailMapper : IBaseMapper<ConjunctionDetailDto, ConjunctionEvent>
{
    private readonly IBaseMapper<AlertSummaryDto, Alert> _alertMapper;
    private readonly IBaseMapper<OrbitDto, Domain.Entities.Orbit> _orbitMapper;

    public ConjunctionDetailMapper(
        IBaseMapper<AlertSummaryDto, Alert> alertMapper,
        IBaseMapper<OrbitDto, Domain.Entities.Orbit> orbitMapper)
    {
        _alertMapper = alertMapper;
        _orbitMapper = orbitMapper;
    }

    public ConjunctionDetailDto? Map(ConjunctionEvent? c, string operatorSide)
    {
        if (c is null) return null;

        return new ConjunctionDetailDto
        {
            Id                       = c.Id,
            IsHighRisk               = c.IsHighRisk(),
            TimeOfClosestApproach    = c.TimeOfClosestApproach,
            PrimaryObjectId          = c.PrimaryObjectId,
            SecondaryObjectId        = c.SecondaryObjectId,
            MissDistance             = c.MissDistance,
            CollisionProbability     = c.CollisionProbability,
            Status                   = c.Status,
            PrimaryAcknowledgement   = c.PrimaryAcknowledgement,
            SecondaryAcknowledgement = c.SecondaryAcknowledgement,
            OperatorSide             = operatorSide,

            PrimaryObjectName   = SpaceObjectResolver.ResolveName(c.PrimaryObject,   c.PrimaryObjectId),
            SecondaryObjectName = SpaceObjectResolver.ResolveName(c.SecondaryObject, c.SecondaryObjectId),

            PrimaryIsSatellite   = c.PrimaryObject   is Satellite,
            SecondaryIsSatellite = c.SecondaryObject is Satellite,

            PrimaryNoradId   = SpaceObjectResolver.ResolveNoradId(c.PrimaryObject),
            SecondaryNoradId = SpaceObjectResolver.ResolveNoradId(c.SecondaryObject),

            PrimaryObjectStatus   = (c.PrimaryObject   as Satellite)?.Status,
            SecondaryObjectStatus = (c.SecondaryObject as Satellite)?.Status,

            PrimaryOrbit   = _orbitMapper.Map(c.PrimaryObject?.Orbit),
            SecondaryOrbit = _orbitMapper.Map(c.SecondaryObject?.Orbit),

            Alerts = c.Alerts
                .OrderByDescending(a => !a.IsAcknowledged)
                .ThenByDescending(a => a.Severity)
                .Select(_alertMapper.Map)
                .OfType<AlertSummaryDto>()
                .ToList()
        };
    }

    public ConjunctionDetailDto? Map(ConjunctionEvent? entity) => Map(entity, "None");
    public ConjunctionEvent? Map(ConjunctionDetailDto? entity) => throw new NotImplementedException();
}