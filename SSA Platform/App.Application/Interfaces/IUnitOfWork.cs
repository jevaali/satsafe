using App.Application.Interfaces.IRepositories;
using Base.Contracts;

namespace App.Application.Interfaces;

public interface IUnitOfWork : IBaseUOW
{
    ISatelliteRepository Satellites { get; }
    IDebrisObjectRepository DebrisObjects { get; }
    IOperatorRepository Operators { get; }
    IUserOperatorRepository UserOperators { get; }
    IConjunctionEventRepository ConjunctionEvents { get; }
    IAlertRepository Alerts { get; }
    ISpaceObjectsRepository SpaceObjects { get; set; }
    INotificationSettingRepository NotificationSettings { get; }
    IDraftSatelliteRepository DraftSatellites { get; }
    IRiskAssessmentRepository RiskAssessments { get; }
    IOperatorActionLogRepository OperatorActionLogs { get; }
    
}
