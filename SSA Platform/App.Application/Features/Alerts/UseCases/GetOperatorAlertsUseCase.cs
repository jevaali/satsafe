using App.Application.Features.Alerts.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Alerts.UseCases;

public class GetOperatorAlertsUseCase : BaseUseCase<AlertSummaryDto, Alert>
{
    public GetOperatorAlertsUseCase(IUnitOfWork uow, IBaseMapper<AlertSummaryDto, Alert> mapper)
        : base(uow, mapper) { }

    public async Task<IEnumerable<AlertSummaryDto>> ExecuteAsync(Guid operatorId)
    {
        var satellites   = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        var satelliteIds = satellites.Select(s => s.Id).ToList();
        var conjunctions = await Uow.ConjunctionEvents.GetBySatelliteIdsAsync(satelliteIds);
        var conjunctionIds = conjunctions.Select(c => c.Id).ToList();
        var alerts       = await Uow.Alerts.GetByConjunctionIdsAsync(conjunctionIds);

        return alerts.Select(Mapper.Map).OfType<AlertSummaryDto>();
    }
}
