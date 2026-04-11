using App.Domain.Entities.Enums;

namespace App.Application.DTO;

public class AlertSummaryDto
{
    public Guid Id { get; init; }
    public bool IsAcknowledged { get; init; }
    public DateTime CreatedAt { get; init; }
    public EAlertSeverity Severity { get; init; }
    public string? Message { get; init; }
}
