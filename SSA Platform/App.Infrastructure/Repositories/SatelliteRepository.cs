using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain;
using App.Domain.Entities;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.Repositories;

public class SatelliteRepository : ISatelliteRepository
{
    private readonly AppDbContext _context;

    public SatelliteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Satellite?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(Satellite satellite)
    {
        await _context.Satellites.AddAsync(satellite);
    }

    public Task UpdateAsync(Satellite entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Satellite entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Satellite>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
    
    public Task Update(Satellite satellite)
    {
        throw new NotImplementedException();
    }
    
}