using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Satellites.DTOs;

namespace App.Application.Features.Operators.DTOs;

public class SettingsDto
{
    public OperatorSummaryDto Operator { get; init; } = null!;
    public IReadOnlyList<SatelliteSummaryDto> Satellites { get; init; } = [];
    public IReadOnlyList<HelperDto> Helpers { get; init; } = [];
    public NotificationSettingDto? NotificationSetting { get; init; }
}
