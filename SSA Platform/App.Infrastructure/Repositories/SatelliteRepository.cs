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

    public async Task AddAsync(Satellite satellite)
    {
        await _context.Satellites.AddAsync(satellite);
    }

    Task<Satellite> ISatelliteRepository.GetByIdAsync(Guid satelliteId)
    {
        throw new NotImplementedException();
    }

    public Task Update(Satellite satellite)
    {
        throw new NotImplementedException();
    }
    
}