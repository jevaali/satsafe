namespace App.Application.DTO;

public class SettingsDto
{
    public OperatorSummaryDto Operator { get; init; } = null!;
    public IReadOnlyList<SatelliteSummaryDto> Satellites { get; init; } = [];
    public IReadOnlyList<HelperDto> Helpers { get; init; } = [];
    public NotificationSettingDto? NotificationSetting { get; init; }
}
