using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class DebrisObjectRepository : IDebrisObjectRepository
{
    private readonly AppDbContext _context;

    public DebrisObjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DebrisObject?> GetByIdAsync(Guid? id)
    {
        return await _context.SpaceObjects
            .OfType<DebrisObject>()
            .Include(d => d.Orbit)
            .Include(d => d.TleRecords)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddAsync(DebrisObject debrisObject)
    { 
        _context.SpaceObjects.Add(debrisObject);
    }

    public async Task UpdateAsync(DebrisObject entity)
    {
        _context.SpaceObjects.Update(entity);
    }

    public async Task DeleteAsync(DebrisObject entity)
    {
        _context.SpaceObjects.Remove(entity);
    }

    public async Task<IEnumerable<DebrisObject>> GetAllAsync()
    {
        return await _context.SpaceObjects
            .OfType<DebrisObject>()
            .Include(d => d.Orbit)
            .Include(d => d.TleRecords)
            .ToListAsync();
    }

    public Task<int> GetCountAsync()
    {
        return _context.SpaceObjects.OfType<DebrisObject>().CountAsync();
    }

    public async Task Update(DebrisObject debrisObject)
    {
        _context.SpaceObjects.Update(debrisObject);
    }
    
}
