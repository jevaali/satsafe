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
    public ISpaceObjectsRepository SpaceObjects { get; set; }
    public INotificationSettingRepository NotificationSettings { get; }
    public IDraftSatelliteRepository DraftSatellites { get; }
    public IRiskAssessmentRepository RiskAssessments { get; }
    public IOperatorActionLogRepository OperatorActionLogs { get; }

    public UnitOfWork(
        AppDbContext context,
        ISatelliteRepository satellites,
        IDebrisObjectRepository debrisObjects,
        IOperatorRepository operators,
        IUserOperatorRepository userOperators,
        IConjunctionEventRepository conjunctionEvents,
        IAlertRepository alerts,
        ISpaceObjectsRepository spaceObjects,
        INotificationSettingRepository notificationSettings,
        IDraftSatelliteRepository draftSatellites,
        IRiskAssessmentRepository riskAssessments,
        IOperatorActionLogRepository operatorActionLogs)
    {
        _context = context;
        Satellites = satellites;
        DebrisObjects = debrisObjects;
        Operators = operators;
        UserOperators = userOperators;
        ConjunctionEvents = conjunctionEvents;
        Alerts = alerts;
        SpaceObjects = spaceObjects;
        NotificationSettings = notificationSettings;
        DraftSatellites = draftSatellites;
        RiskAssessments = riskAssessments;
        OperatorActionLogs = operatorActionLogs;
    }

    public async Task<int> CommitAsync()
    {
        return await _context.SaveChangesAsync();
    }
}