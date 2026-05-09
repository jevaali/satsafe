using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Satellites.UseCases;

public class GetOperatorSatellitesUseCase : BaseUseCase<SatelliteSummaryDto, Satellite>
{
    public GetOperatorSatellitesUseCase(IUnitOfWork uow, IBaseMapper<SatelliteSummaryDto, Satellite> mapper)
        : base(uow, mapper) { }

    public async Task<IEnumerable<SatelliteSummaryDto>> ExecuteAsync(Guid operatorId)
    {
        var satellites = await Uow.Satellites.GetByOperatorIdAsync(operatorId);
        return satellites.Select(Mapper.Map).OfType<SatelliteSummaryDto>();
    }
}
