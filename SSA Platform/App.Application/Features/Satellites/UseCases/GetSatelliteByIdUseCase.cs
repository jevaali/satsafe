using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Satellites.UseCases;

public class GetSatelliteByIdUseCase : BaseUseCase<SatelliteDetailDto, Satellite>
{
    public GetSatelliteByIdUseCase(IUnitOfWork uow, IBaseMapper<SatelliteDetailDto, Satellite> mapper)
        : base(uow, mapper) { }

    public async Task<SatelliteDetailDto?> ExecuteAsync(Guid satelliteId, Guid operatorId)
    {
        var satellite = await Uow.Satellites.FindAsync(satelliteId);

        if (satellite == null || satellite.OperatorId != operatorId)
            return null;

        return Mapper.Map(satellite);
    }
}
