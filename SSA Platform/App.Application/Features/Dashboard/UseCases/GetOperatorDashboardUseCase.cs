using App.Application.Features.Alerts.DTOs;
using App.Application.Features.Conjunctions.DTOs;
using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using Base.Contracts;

namespace App.Application.Features.Dashboard.UseCases;

public class GetOperatorDashboardUseCase : BaseUseCase
{
    private readonly IBaseMapper<SatelliteSummaryDto, Satellite> _satelliteMapper;
    private readonly IBaseMapper<ConjunctionSummaryDto, ConjunctionEvent> _conjunctionMapper;
    private readonly IBaseMapper<AlertSummaryDto, Alert> _alertMapper;

    public GetOperatorDashboardUseCase(
        IUnitOfWork uow,
        IBaseMapper<SatelliteSummaryDto, Satellite> satelliteMapper,
        IBaseMapper<ConjunctionSummaryDto, ConjunctionEvent> conjunctionMapper,
        IBaseMapper<AlertSummaryDto, Alert> alertMapper) : base(uow)
    {
        _satelliteMapper   = satelliteMapper;
        _conjunctionMapper = conjunctionMapper;
        _alertMapper       = alertMapper;
    }

    public async Task<OperatorDashboardDto> ExecuteAsync(Guid userId, Guid operatorId)
    {
        if (userId == Guid.Empty || operatorId == Guid.Empty)
            throw new ArgumentException("Invalid user or operator ID.");

        if (await Uow.UserOperators.GetByUserAndOperatorAsync(userId, operatorId) == null)
            throw new UnauthorizedAccessException("User does not have access to this operator's dashboard.");

        var satellites     = (await Uow.Satellites.GetByOperatorIdAsync(operatorId)).ToList();
        var satelliteIds   = satellites.Select(s => s.Id).ToList();

        var conjunctions   = (await Uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds)).ToArray();
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alerts         = (await Uow.Alerts.GetByConjunctionIdsAsync(conjunctionIds)).ToArray();

        var operatorEntity = await Uow.Operators.FindAsync(operatorId);

        return new OperatorDashboardDto
        {
            OperatorName         = operatorEntity?.Name.ToString() ?? "Operator",
            TotalSatellites      = satellites.Count,
            ActiveSatellites     = satellites.Count(s => s.Status == ESatelliteStatus.Active),
            PendingConjunctions  = conjunctions.Count(c => c.Status == EConjunctionEventStatus.Pending),
            HighRiskConjunctions = conjunctions.Count(c => c.IsHighRisk()),
            UnacknowledgedAlerts = alerts.Count(a => !a.IsAcknowledged),

            Satellites   = satellites.Select(_satelliteMapper.Map).OfType<SatelliteSummaryDto>().ToList(),
            Conjunctions = conjunctions.Select(_conjunctionMapper.Map).OfType<ConjunctionSummaryDto>().ToList(),
            Alerts       = alerts.Select(_alertMapper.Map).OfType<AlertSummaryDto>().ToList()
        };
    }
}
