using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class DraftSatelliteRepository : IDraftSatelliteRepository
{
    private readonly AppDbContext _context;

    public DraftSatelliteRepository(AppDbContext context) => _context = context;

    public async Task<DraftSatellite?> GetByIdAsync(Guid? id)
        => await _context.DraftSatellites.FirstOrDefaultAsync(d => d.Id == id);

    public async Task AddAsync(DraftSatellite entity)
        => _context.DraftSatellites.Add(entity);

    public async Task UpdateAsync(DraftSatellite entity)
        => _context.DraftSatellites.Update(entity);

    public async Task DeleteAsync(DraftSatellite entity)
        => _context.DraftSatellites.Remove(entity);

    public async Task<IEnumerable<DraftSatellite>> GetAllAsync()
        => await _context.DraftSatellites.ToListAsync();

    public async Task<IEnumerable<DraftSatellite>> GetByUserIdAsync(Guid userId)
        => await _context.DraftSatellites
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync();

    public async Task<DraftSatellite?> GetByIdAndUserAsync(Guid id, Guid userId)
        => await _context.DraftSatellites
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
}
