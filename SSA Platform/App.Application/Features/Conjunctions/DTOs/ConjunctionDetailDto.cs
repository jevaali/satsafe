using App.Application.Common.DTOs;
using App.Application.Features.Alerts.DTOs;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Conjunctions.DTOs;

public class ConjunctionDetailDto
{
    public Guid Id { get; init; }
    public bool IsHighRisk { get; init; }
    public DateTime TimeOfClosestApproach { get; init; }
    public Guid PrimaryObjectId { get; init; }
    public Guid SecondaryObjectId { get; init; }
    public string PrimaryObjectName { get; init; } = string.Empty;
    public string SecondaryObjectName { get; init; } = string.Empty;
    public double MissDistance { get; init; }
    public double CollisionProbability { get; init; }
    public EConjunctionEventStatus Status { get; init; }
    public EOperatorAcknowledgement PrimaryAcknowledgement { get; init; }
    public EOperatorAcknowledgement SecondaryAcknowledgement { get; init; }

    /// <summary>"Primary", "Secondary", or "None" — which side of this event belongs to the requesting operator.</summary>
    public string OperatorSide { get; init; } = "None";

    // Object type metadata for display
    public bool PrimaryIsSatellite { get; init; }
    public bool SecondaryIsSatellite { get; init; }
    public int? PrimaryNoradId { get; init; }
    public int? SecondaryNoradId { get; init; }
    public ESatelliteStatus? PrimaryObjectStatus { get; init; }
    public ESatelliteStatus? SecondaryObjectStatus { get; init; }

    // Orbital parameters for visualisation (null when not available)
    public OrbitDto? PrimaryOrbit { get; init; }
    public OrbitDto? SecondaryOrbit { get; init; }

    public IReadOnlyList<AlertSummaryDto> Alerts { get; init; } = [];
}
