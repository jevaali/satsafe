using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IConjunctionEventRepository : IRepository<ConjunctionEvent>
{
    Task<IEnumerable<ConjunctionEvent>> GetBySatelliteIdsAsync(List<Guid> satelliteIds);
}
