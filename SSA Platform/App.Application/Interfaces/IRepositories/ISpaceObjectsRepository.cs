using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface ISpaceObjectsRepository : IRepository<SpaceObject>
{
    Task<IEnumerable<SpaceObject>> GetAllWithOrbitsAsync();
}