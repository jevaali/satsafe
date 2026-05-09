using App.Application.Features.Notifications.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Notifications.UseCases;

public class SaveNotificationsUseCase : BaseUseCase
{
    public SaveNotificationsUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task ExecuteAsync(Guid userId, SaveNotificationsRequest request)
    {
        var setting = await Uow.NotificationSettings.GetByUserIdAsync(userId);

        if (setting == null)
        {
            setting = new NotificationSetting
            {
                Id                     = Guid.NewGuid(),
                AppUserId              = userId,
                ConjunctionThresholdKm = request.ConjunctionThresholdKm,
                AlertThresholdKm       = request.AlertThresholdKm,
                EmailEnabled           = request.EmailEnabled,
                SmsEnabled             = request.SmsEnabled,
                CreatedAt              = DateTime.UtcNow,
                CreatedBy              = userId.ToString()
            };
            Uow.NotificationSettings.Add(setting);
        }
        else
        {
            setting.ConjunctionThresholdKm = request.ConjunctionThresholdKm;
            setting.AlertThresholdKm       = request.AlertThresholdKm;
            setting.EmailEnabled           = request.EmailEnabled;
            setting.SmsEnabled             = request.SmsEnabled;
            await Uow.NotificationSettings.UpdateAsync(setting);
        }

        await Uow.CommitAsync();
    }
}
