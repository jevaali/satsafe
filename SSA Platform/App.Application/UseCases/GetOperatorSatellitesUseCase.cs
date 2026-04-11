using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class GetOperatorSatellitesUseCase
{
    private readonly IUnitOfWork _uow;

    public GetOperatorSatellitesUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Satellite>> ExecuteAsync(Guid operatorId)
    {
        return await _uow.Satellites.GetByOperatorIdAsync(operatorId);
    }
}
