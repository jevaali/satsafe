using App.Application.Interfaces.IRepositories;

namespace App.Application.Interfaces;

public interface IUnitOfWork
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

    Task CommitAsync();
}
