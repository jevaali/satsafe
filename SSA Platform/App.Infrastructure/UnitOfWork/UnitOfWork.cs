using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ISatelliteRepository Satellites { get; }
    public IOperatorRepository Operators { get; }

    public UnitOfWork(
        AppDbContext context,
        ISatelliteRepository satellites,
        IOperatorRepository operators)
    {
        _context = context;
        Satellites = satellites;
        Operators = operators;
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }
}