using App.Application.Features.Dashboard.DTOs;
using App.Application.Features.Notifications.DTOs;
using App.Application.Features.Operators.DTOs;

namespace WebApp.Areas.Operator.Models;

public class SettingsViewModel
{
    public SettingsDto Settings { get; set; } = null!;

    public UpdateOperatorSettingsRequest OperatorForm { get; set; } = new();
    public AddHelperRequest HelperForm { get; set; } = new();
    public SaveNotificationsRequest NotificationForm { get; set; } = new();

    public IEnumerable<OperatorActionLogDto> ActionLogs { get; set; } = [];
    public Guid? ActivityFilterUserId { get; set; }
}
