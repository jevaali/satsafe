using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Base.Application.EF;
using Base.Contracts;
using Base.EF;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class NotificationSettingRepository : BaseRepository<NotificationSetting, NotificationSetting, AppDbContext>, INotificationSettingRepository
{
    public NotificationSettingRepository(AppDbContext context, IBaseMapper<NotificationSetting, NotificationSetting> mapper)
        : base(context, mapper)
    {
    }
    public async Task<NotificationSetting?> GetByUserIdAsync(Guid userId)
        => await RepositoryDbSet.FirstOrDefaultAsync(n => n.AppUserId == userId);
}