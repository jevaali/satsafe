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
    public IUserOperatorRepository UserOperators { get; }
    public IConjunctionEventRepository ConjunctionEvents { get; }
    public IAlertRepository Alerts { get; }

    public UnitOfWork(
        AppDbContext context,
        ISatelliteRepository satellites,
        IDebrisObjectRepository debrisObjects,
        IOperatorRepository operators,
        IUserOperatorRepository userOperators,
        IConjunctionEventRepository conjunctionEvents,
        IAlertRepository alerts)
    {
        _context = context;
        Satellites = satellites;
        DebrisObjects = debrisObjects;
        Operators = operators;
        UserOperators = userOperators;
        ConjunctionEvents = conjunctionEvents;
        Alerts = alerts;
    }

    public async Task CommitAsync()
    {
        await _context.SaveChangesAsync();
    }
}