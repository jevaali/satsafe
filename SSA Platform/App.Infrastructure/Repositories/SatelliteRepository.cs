using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class SatelliteRepository : ISatelliteRepository
{
    private readonly AppDbContext _context;

    public SatelliteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Satellite?> GetByIdAsync(Guid? id)
    {
        return await _context.SpaceObjects
            .OfType<Satellite>()
            .Include(s => s.Operator)
            .Include(s => s.Orbit)
            .Include(s => s.TleRecords)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Satellite satellite)
    { 
        _context.SpaceObjects.Add(satellite);
    }

    public async Task UpdateAsync(Satellite entity)
    {
        _context.SpaceObjects.Update(entity);
    }

    public async Task DeleteAsync(Satellite entity)
    {
        _context.SpaceObjects.Remove(entity);
    }

    public async Task<IEnumerable<Satellite>> GetAllAsync()
    {
        return await _context.SpaceObjects
            .OfType<Satellite>()
            .Include(s => s.Operator)
            .Include(s => s.Orbit)
            .Include(s => s.TleRecords)
            .ToListAsync();
    }

    public async Task<IEnumerable<Satellite>> GetByOperatorIdAsync(Guid operatorId)
    {
        return await _context.SpaceObjects
            .OfType<Satellite>()
            .Include(s => s.Operator)
            .Include(s => s.Orbit)
            .Include(s => s.TleRecords)
            .Where(s => s.OperatorId == operatorId)
            .ToListAsync();
    }

    public Task<int> GetCountAsync()
    {
        return _context.SpaceObjects.OfType<Satellite>().CountAsync();
    }

    public Task<int> GetActiveCountAsync()
    {
        return _context.SpaceObjects.OfType<Satellite>().CountAsync(s => s.Status == ESatelliteStatus.Active);
    }

    public Task<List<Satellite>> GetTopAsync(int count)
    {
        return _context.SpaceObjects
            .OfType<Satellite>()
            .Include(s => s.Operator)
            .Take(count)
            .ToListAsync();
    }

    public async Task Update(Satellite satellite)
    {
        _context.SpaceObjects.Update(satellite);
    }
    
}