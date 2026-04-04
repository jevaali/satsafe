using App.Domain.Entities;

namespace App.Application.Interfaces.IRepositories;

public interface INotificationSettingRepository : IRepository<NotificationSetting>
{
    Task<NotificationSetting?> GetByUserIdAsync(Guid userId);
}
