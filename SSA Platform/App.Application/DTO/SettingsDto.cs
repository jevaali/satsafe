using App.Domain.Entities;

namespace App.Application.DTO;

public class SettingsDto
{
    public Operator Operator { get; set; } = null!;
    public IEnumerable<Satellite> Satellites { get; set; } = [];
    public IEnumerable<UserOperator> Helpers { get; set; } = [];
    public NotificationSetting? NotificationSetting { get; set; }
}
