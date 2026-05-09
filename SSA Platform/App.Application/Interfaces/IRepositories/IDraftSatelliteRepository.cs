using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface IDraftSatelliteRepository : IBaseRepository<DraftSatellite>
{
    Task<IEnumerable<DraftSatellite>> GetByUserIdAsync(Guid userId);
    Task<DraftSatellite?> GetByIdAndUserAsync(Guid id, Guid userId);
}
