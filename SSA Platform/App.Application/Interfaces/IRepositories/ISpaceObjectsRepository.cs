using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface ISpaceObjectsRepository : IBaseRepository<SpaceObject>
{
    Task<IEnumerable<SpaceObject>> GetAllWithOrbitsAsync();
}