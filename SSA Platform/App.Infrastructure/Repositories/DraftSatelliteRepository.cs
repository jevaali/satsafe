using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class DraftSatelliteRepository : BaseRepository<DraftSatellite, DraftSatellite, AppDbContext>, IDraftSatelliteRepository
{
    public DraftSatelliteRepository(AppDbContext context, IBaseMapper<DraftSatellite, DraftSatellite> mapper)
        : base(context, mapper)
    {
    }
    
    public async Task<IEnumerable<DraftSatellite>> GetByUserIdAsync(Guid userId)
        => await RepositoryDbSet
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync();

    public async Task<DraftSatellite?> GetByIdAndUserAsync(Guid id, Guid userId)
        => await RepositoryDbSet
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
}