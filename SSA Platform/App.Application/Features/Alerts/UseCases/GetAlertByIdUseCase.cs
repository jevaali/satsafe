using App.Application.Features.Alerts.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Alerts.UseCases;

public class GetAlertByIdUseCase : BaseUseCase<AlertDetailsDto, Alert>
{
    public GetAlertByIdUseCase(IUnitOfWork uow, IBaseMapper<AlertDetailsDto, Alert> mapper)
        : base(uow, mapper) { }

    public async Task<AlertDetailsDto?> ExecuteAsync(Guid alertId, Guid operatorId)
    {
        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await Uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();

        var alert = await Uow.Alerts.FindAsync(alertId);
        if (alert == null || !conjunctionIds.Contains(alert.ConjunctionEventId))
            return null;

        return Mapper.Map(alert);
    }
}
