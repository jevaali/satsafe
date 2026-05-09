using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class DebrisObjectRepository : BaseRepository<DebrisObject, DebrisObject, AppDbContext>, IDebrisObjectRepository
{
    public DebrisObjectRepository(AppDbContext context, IBaseMapper<DebrisObject, DebrisObject> mapper)
        : base(context, mapper)
    {
    }

    public async Task<DebrisObject?> GetByIdAsync(Guid? id)
    {
        return await RepositoryDbSet
            .Include(d => d.Orbit)
            .Include(d => d.TleRecords)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<DebrisObject>> GetAllAsync()
    {
        return await RepositoryDbSet
            .Include(d => d.Orbit)
            .Include(d => d.TleRecords)
            .ToListAsync();
    }

    public Task<int> GetCountAsync()
    {
        return RepositoryDbSet.CountAsync();
    }
}