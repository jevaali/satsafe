using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class ConjunctionEventRepository : IConjunctionEventRepository
{
    private readonly AppDbContext _context;

    public ConjunctionEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConjunctionEvent?> GetByIdAsync(Guid? id)
    {
        return await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .Include(c => c.RiskAssessment)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(ConjunctionEvent entity)
    {
        _context.ConjunctionEvents.Add(entity);
    }

    public async Task UpdateAsync(ConjunctionEvent entity)
    {
        _context.ConjunctionEvents.Update(entity);
    }

    public async Task DeleteAsync(ConjunctionEvent entity)
    {
        _context.ConjunctionEvents.Remove(entity);
    }

    public async Task<IEnumerable<ConjunctionEvent>> GetAllAsync()
    {
        return await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConjunctionEvent>> GetBySatelliteIdsAsync(List<Guid> satelliteIds)
    {
        return await _context.ConjunctionEvents
            .Include(c => c.PrimaryObject)
            .Include(c => c.SecondaryObject)
            .Include(c => c.RiskAssessment)
            .Include(c => c.Alerts)
            .Where(c => satelliteIds.Contains(c.PrimaryObjectId) || satelliteIds.Contains(c.SecondaryObjectId))
            .ToListAsync();
    }
}
