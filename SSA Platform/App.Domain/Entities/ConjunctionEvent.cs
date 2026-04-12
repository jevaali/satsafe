using App.Domain.Entities.Enums;
using Base.Domain;

namespace App.Domain.Entities;

public class ConjunctionEvent : BaseEntityWithMeta
{
    public Guid PrimaryObjectId { get; set; }
    public SpaceObject? PrimaryObject { get; set; }
    
    public Guid SecondaryObjectId { get; set; }
    public SpaceObject? SecondaryObject { get; set; }
    
    public DateTime TimeOfClosestApproach { get; set; }
    public double MissDistance { get; set; }
    public double CollisionProbability { get; set; }
    public EConjunctionEventStatus Status { get; set; }

    public EOperatorAcknowledgement PrimaryAcknowledgement   { get; private set; } = EOperatorAcknowledgement.None;
    public EOperatorAcknowledgement SecondaryAcknowledgement { get; private set; } = EOperatorAcknowledgement.None;

    public ICollection<Alert> Alerts { get; set; } = [];
    
    public double Threshold { get; private set; }

    public bool IsThreat => MissDistance < Threshold;

    public ConjunctionEvent(
        Guid primaryObjectId,
        Guid secondaryObjectId,
        DateTime timeOfClosestApproach,
        double missDistance,
        double threshold)
    {
        PrimaryObjectId        = primaryObjectId;
        SecondaryObjectId      = secondaryObjectId;
        TimeOfClosestApproach  = timeOfClosestApproach;
        MissDistance           = missDistance;
        Threshold              = threshold;
        Status                 = EConjunctionEventStatus.Pending;
        CreatedBy              = "system";
        CreatedAt              = DateTime.UtcNow;
    }

    private ConjunctionEvent() { }
    
    public void UpdateMissDistance(double distance)
    {
        if (distance < 0)
            throw new ArgumentException("Miss distance cannot be negative");

        MissDistance = distance;
    }

    
    public void UpdateCollisionProbability(double probability)
    {
        if (probability is < 0 or > 1)
            throw new ArgumentException("Probability must be between 0 and 1");

        CollisionProbability = probability;
    }
    
    
    public bool IsHighRisk()
    {
        return CollisionProbability > 0.01 || MissDistance < 1000; // example thresholds
    }
    
    
    public void RefreshStatus()
    {
        if (Status == EConjunctionEventStatus.Resolved) return;

        var timeUntilTca = TimeOfClosestApproach - DateTime.UtcNow;
        Status = timeUntilTca.TotalSeconds < 0
            ? EConjunctionEventStatus.AlreadyOccurred
            : timeUntilTca.TotalHours < 24
                ? EConjunctionEventStatus.Imminent
                : timeUntilTca.TotalHours < 72
                    ? EConjunctionEventStatus.Potential
                    : EConjunctionEventStatus.Pending;
    }

    public void AcknowledgePrimarySide(EOperatorAcknowledgement state)
    {
        if (state <= PrimaryAcknowledgement)
            throw new InvalidOperationException("Cannot move acknowledgement backwards.");
        PrimaryAcknowledgement = state;
        AutoResolveIfBothSidesDone();
    }

    public void AcknowledgeSecondarySide(EOperatorAcknowledgement state)
    {
        if (state <= SecondaryAcknowledgement)
            throw new InvalidOperationException("Cannot move acknowledgement backwards.");
        SecondaryAcknowledgement = state;
        AutoResolveIfBothSidesDone();
    }

    private void AutoResolveIfBothSidesDone()
    {
        if (PrimaryAcknowledgement   == EOperatorAcknowledgement.Resolved &&
            SecondaryAcknowledgement == EOperatorAcknowledgement.Resolved &&
            Status != EConjunctionEventStatus.AlreadyOccurred)
        {
            Status = EConjunctionEventStatus.Resolved;
        }
    }

    public void MarkAsResolved()
    {
        if (Status == EConjunctionEventStatus.AlreadyOccurred)
            throw new InvalidOperationException("Cannot resolve past events");

        Status = EConjunctionEventStatus.Resolved;
    }

    /// <summary>
    /// Returns a normalised (smaller, larger) pair used for deduplication.
    /// </summary>
    public (Guid Min, Guid Max) GetOrderedPair() =>
        PrimaryObjectId < SecondaryObjectId
            ? (PrimaryObjectId, SecondaryObjectId)
            : (SecondaryObjectId, PrimaryObjectId);

    /// <summary>
    /// Creates an Alert for this event if the miss-distance is within the alert
    /// threshold, or returns null when no alert is warranted.
    /// </summary>
    public Alert? CreateAlert(Guid userId, double alertThresholdKm)
    {
        var alertThresholdM = alertThresholdKm * 1000;
        
        if (MissDistance > alertThresholdM) return null;

        var severity = MissDistance <= alertThresholdM / 5
            ? EAlertSeverity.Critical
            : EAlertSeverity.High;

        return new Alert
        {
            ConjunctionEventId = Id,
            UserId             = userId,
            Severity           = severity,
            Message            = $"Conjunction detected: miss distance " +
                                 $"{MissDistance / 1000:F2} km in " +
                                 $"{(TimeOfClosestApproach - DateTime.UtcNow).TotalHours:F1}h",
            IsAcknowledged     = false,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = "system"
        };
    }
}
