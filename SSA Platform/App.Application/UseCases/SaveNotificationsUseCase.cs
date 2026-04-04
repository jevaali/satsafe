using App.Application.DTO;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class SaveNotificationsUseCase
{
    private readonly IUnitOfWork _uow;

    public SaveNotificationsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task ExecuteAsync(Guid userId, SaveNotificationsRequest request)
    {
        var setting = await _uow.NotificationSettings.GetByUserIdAsync(userId);

        if (setting == null)
        {
            setting = new NotificationSetting
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                ThresholdDistance = request.ThresholdDistance,
                EmailEnabled = request.EmailEnabled,
                SmsEnabled = request.SmsEnabled,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };
            await _uow.NotificationSettings.AddAsync(setting);
        }
        else
        {
            setting.ThresholdDistance = request.ThresholdDistance;
            setting.EmailEnabled = request.EmailEnabled;
            setting.SmsEnabled = request.SmsEnabled;
            await _uow.NotificationSettings.UpdateAsync(setting);
        }

        await _uow.CommitAsync();
    }
}
