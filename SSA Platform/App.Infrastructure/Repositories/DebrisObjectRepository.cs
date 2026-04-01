using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain;
using App.Domain.Entities;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.Repositories;

public class DebrisObjectRepository : IDebrisObjectRepository
{
    private readonly AppDbContext _context;

    public DebrisObjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<DebrisObject?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(DebrisObject debrisObject)
    { 
        _context.SpaceObjects.Add(debrisObject);
    }

    public Task UpdateAsync(DebrisObject entity)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(DebrisObject entity)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<DebrisObject>> GetAllAsync()
    {
        throw new NotImplementedException();
    }
    
    public Task Update(DebrisObject debrisObject)
    {
        throw new NotImplementedException();
    }
    
}
