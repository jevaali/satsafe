using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Satellites.UseCases;

public class GetSatelliteByIdUseCase
{
    private readonly IUnitOfWork _uow;

    public GetSatelliteByIdUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Satellite?> ExecuteAsync(Guid satelliteId, Guid operatorId)
    {
        var satellite = await _uow.Satellites.GetByIdAsync(satelliteId);
        if (satellite == null || satellite.OperatorId != operatorId)
            return null;
        return satellite;
    }
}
