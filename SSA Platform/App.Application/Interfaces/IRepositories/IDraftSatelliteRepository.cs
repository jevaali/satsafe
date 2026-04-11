using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface IDraftSatelliteRepository : IRepository<DraftSatellite>
{
    Task<IEnumerable<DraftSatellite>> GetByUserIdAsync(Guid userId);
    Task<DraftSatellite?> GetByIdAndUserAsync(Guid id, Guid userId);
}
