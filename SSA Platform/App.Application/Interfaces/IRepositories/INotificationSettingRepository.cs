using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Interfaces.IRepositories;

public interface INotificationSettingRepository : IBaseRepository<NotificationSetting>
{
    Task<NotificationSetting?> GetByUserIdAsync(Guid userId);
}
