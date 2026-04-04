using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetSettingsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetSettingsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SettingsDto> ExecuteAsync(Guid userId, Guid operatorId)
    {
        var operatorWithDetails = await _uow.Operators.GetWithDetailsAsync(operatorId)
            ?? throw new Exception("Operator not found");

        var notificationSetting = await _uow.NotificationSettings.GetByUserIdAsync(userId);

        return new SettingsDto
        {
            Operator = operatorWithDetails,
            Satellites = operatorWithDetails.Satellites,
            Helpers = operatorWithDetails.UserOperators,
            NotificationSetting = notificationSetting
        };
    }
}
