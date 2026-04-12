using App.Domain.Entities.Enums;

namespace App.Application.Features.Alerts.DTOs;

public class AlertDetailsDto
{
    public Guid Id { get; init; }
    public bool IsAcknowledged { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public EAlertSeverity Severity { get; init; }
    public string? Message { get; init; }
    public Guid ConjunctionEventId { get; init; }
    public ConjunctionDetailsDto? ConjunctionEvent { get; init; }
}

public class ConjunctionDetailsDto
{
    public Guid Id { get; init; }
    public DateTime TimeOfClosestApproach { get; init; }
    public double MissDistance { get; init; }
    public double CollisionProbability { get; init; }
    public bool IsHighRisk { get; init; }
    public EConjunctionEventStatus Status { get; init; }
}
