using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class SpaceObjectsRepository : BaseRepository<SpaceObject, SpaceObject, AppDbContext>, ISpaceObjectsRepository
{
    public SpaceObjectsRepository(AppDbContext context, IBaseMapper<SpaceObject, SpaceObject> mapper)
        : base(context, mapper)
    {
    }
    
    public async Task<IEnumerable<SpaceObject>> GetAllWithOrbitsAsync()
        => await RepositoryDbSet
            .Include(so => so.Orbit)
            .Where(so => so.Orbit != null)
            .ToListAsync();
}