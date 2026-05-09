using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IConjunctionEventRepository : IBaseRepository<ConjunctionEvent>
{
    Task<IEnumerable<ConjunctionEvent>> GetBySatelliteIdsAsync(List<Guid> satelliteIds);
}
