using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ISatelliteRepository Satellites { get; }
    public IDebrisObjectRepository DebrisObjects { get; }
    public IOperatorRepository Operators { get; }

    public UnitOfWork(
        AppDbContext context,
        ISatelliteRepository satellites,
        IDebrisObjectRepository debrisObjects,
        IOperatorRepository operators)
    {
        _context = context;
        Satellites = satellites;
        DebrisObjects = debrisObjects;
        Operators = operators;
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }
}