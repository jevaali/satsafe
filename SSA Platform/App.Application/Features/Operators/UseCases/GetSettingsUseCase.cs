using App.Application.Common.DTOs;
using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Operators.DTOs;
using App.Application.Features.Satellites.DTOs;
using App.Application.Interfaces;

namespace App.Application.Features.Operators.UseCases;

public class GetSettingsUseCase
{
    private readonly IUnitOfWork _uow;

    public GetSettingsUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<SettingsDto> ExecuteAsync(Guid userId, Guid operatorId)
    {
        var op = await _uow.Operators.GetWithDetailsAsync(operatorId)
            ?? throw new Exception("Operator not found");

        var notificationSetting = await _uow.NotificationSettings.GetByUserIdAsync(userId);

        return new SettingsDto
        {
            Operator = new OperatorSummaryDto
            {
                Id = op.Id,
                Name = op.Name.ToString(),
                CountryCode = op.CountryCode,
                OperatorType = op.OperatorType
            },
            Satellites = op.Satellites
                .Select(s => new SatelliteSummaryDto
                {
                    Id = s.Id,
                    Name = s.Name.ToString(),
                    NoradId = s.NoradId,
                    LaunchDate = s.LaunchDate,
                    Status = s.Status,
                    RiskLevel = s.RiskAssessment?.RiskLevel,
                    Orbit = s.Orbit == null ? null : new OrbitDto
                    {
                        SemiMajorAxis     = s.Orbit.Parameters.SemiMajorAxis,
                        Eccentricity      = s.Orbit.Parameters.Eccentricity,
                        Inclination       = s.Orbit.Parameters.Inclination,
                        RAAN              = s.Orbit.Parameters.RAAN,
                        ArgumentOfPerigee = s.Orbit.Parameters.ArgumentOfPerigee,
                        MeanAnomaly       = s.Orbit.Parameters.MeanAnomaly
                    }
                })
                .ToList(),
            Helpers = op.UserOperators
                .Select(uo => new HelperDto
                {
                    Id        = uo.Id,
                    Email     = uo.AppUser?.Email ?? string.Empty,
                    FirstName = uo.AppUser?.FirstName ?? string.Empty,
                    LastName  = uo.AppUser?.LastName ?? string.Empty
                })
                .ToList(),
            NotificationSetting = notificationSetting == null ? null : new NotificationSettingDto
            {
                ConjunctionThresholdKm = notificationSetting.ConjunctionThresholdKm,
                AlertThresholdKm       = notificationSetting.AlertThresholdKm,
                EmailEnabled           = notificationSetting.EmailEnabled,
                SmsEnabled             = notificationSetting.SmsEnabled
            }
        };
    }
}
