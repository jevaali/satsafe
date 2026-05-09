using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class ConjunctionEventRepository : BaseRepository<ConjunctionEvent, ConjunctionEvent, AppDbContext>, IConjunctionEventRepository
{
    public ConjunctionEventRepository(AppDbContext context, IBaseMapper<ConjunctionEvent, ConjunctionEvent> mapper)
        : base(context, mapper)
    {
    }

    public override async Task<ConjunctionEvent?> FindAsync(Guid id, Guid appUserId = default)
        => await RepositoryDbSet
            .Include(c => c.PrimaryObject).ThenInclude(o => o!.Orbit)
            .Include(c => c.SecondaryObject).ThenInclude(o => o!.Orbit)
            .Include(c => c.Alerts)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<ConjunctionEvent>> GetBySatelliteIdsAsync(List<Guid> satelliteIds)
    {
        return await RepositoryDbSet
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .Include(c => c.Alerts)
            .Where(c => satelliteIds.Contains(c.PrimaryObjectId) || satelliteIds.Contains(c.SecondaryObjectId))
            .ToListAsync();
    }
}