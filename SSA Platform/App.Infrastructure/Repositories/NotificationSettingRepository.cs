using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class NotificationSettingRepository : INotificationSettingRepository
{
    private readonly AppDbContext _context;

    public NotificationSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationSetting?> GetByIdAsync(Guid? id) =>
        await _context.NotificationSettings.FindAsync(id);

    public async Task<NotificationSetting?> GetByUserIdAsync(Guid userId) =>
        await _context.NotificationSettings.FirstOrDefaultAsync(n => n.AppUserId == userId);

    public async Task AddAsync(NotificationSetting entity) =>
        await _context.NotificationSettings.AddAsync(entity);

    public Task UpdateAsync(NotificationSetting entity)
    {
        _context.NotificationSettings.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(NotificationSetting entity)
    {
        _context.NotificationSettings.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<NotificationSetting>> GetAllAsync() =>
        await _context.NotificationSettings.ToListAsync();
}
