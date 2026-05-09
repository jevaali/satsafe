using App.Application.Features.Notifications.DTOs;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Mappers.Notifications;

public class NotificationMapper : IBaseMapper<NotificationSettingDto, NotificationSetting>
{
    public NotificationSettingDto? Map(NotificationSetting? s)
    {
        if (s is null) return null;

        return new NotificationSettingDto
        {
            ConjunctionThresholdKm = s.ConjunctionThresholdKm,
            AlertThresholdKm       = s.AlertThresholdKm,
            EmailEnabled           = s.EmailEnabled,
            SmsEnabled             = s.SmsEnabled
        };
    }

    public NotificationSetting? Map(NotificationSettingDto? entity) => throw new NotImplementedException();
}