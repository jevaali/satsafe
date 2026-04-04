using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class SpaceObjectsRepository : ISpaceObjectsRepository
{
    private readonly AppDbContext _context;

    public SpaceObjectsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SpaceObject?> GetByIdAsync(Guid? id)
    {
        return await _context.SpaceObjects
            .Include(so => so.Orbit)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task AddAsync(SpaceObject entity)
    {
        await _context.SpaceObjects.AddAsync(entity);
    }

    public async Task UpdateAsync(SpaceObject entity)
    {
        _context.SpaceObjects.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(SpaceObject entity)
    {
        _context.SpaceObjects.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<SpaceObject>> GetAllAsync()
    {
        return await _context.SpaceObjects
            .Include(so => so.Orbit)
            .ToListAsync();
    }

    public async Task<IEnumerable<SpaceObject>> GetAllWithOrbitsAsync()
    {
        return await _context.SpaceObjects
            .Include(so => so.Orbit)
            .Where(so => so.Orbit != null)  // only objects that have an orbit set
            .ToListAsync();
    }
}