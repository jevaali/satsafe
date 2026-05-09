using App.Application.Common.DTOs;
using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Operators.UseCases;

public class GetSettingsUseCase : BaseUseCase
{
    private readonly IBaseMapper<OperatorSummaryDto, Operator> _operatorMapper;
    private readonly IBaseMapper<SatelliteSummaryDto, Satellite> _satelliteMapper;
    private readonly IBaseMapper<NotificationSettingDto, NotificationSetting> _notificationMapper;

    public GetSettingsUseCase(
        IUnitOfWork uow,
        IBaseMapper<OperatorSummaryDto, Operator> operatorMapper,
        IBaseMapper<SatelliteSummaryDto, Satellite> satelliteMapper,
        IBaseMapper<NotificationSettingDto, NotificationSetting> notificationMapper) : base(uow)
    {
        _operatorMapper     = operatorMapper;
        _satelliteMapper    = satelliteMapper;
        _notificationMapper = notificationMapper;
    }

    public async Task<SettingsDto> ExecuteAsync(Guid userId, Guid operatorId)
    {
        var op = await Uow.Operators.GetWithDetailsAsync(operatorId)
                 ?? throw new Exception("Operator not found");

        var notificationSetting = await Uow.NotificationSettings.GetByUserIdAsync(userId);

        return new SettingsDto
        {
            Operator            = _operatorMapper.Map(op),
            Satellites          = op.Satellites.Select(_satelliteMapper.Map).OfType<SatelliteSummaryDto>().ToList(),
            NotificationSetting = _notificationMapper.Map(notificationSetting),
            Helpers             = op.UserOperators
                .Select(uo => new HelperDto
                {
                    Id        = uo.Id,
                    Email     = uo.AppUser?.Email     ?? string.Empty,
                    FirstName = uo.AppUser?.FirstName ?? string.Empty,
                    LastName  = uo.AppUser?.LastName  ?? string.Empty
                })
                .ToList()
        };
    }
}
