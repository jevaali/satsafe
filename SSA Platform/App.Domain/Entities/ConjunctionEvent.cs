using App.Domain.Entities.Enums;
using Base.Domain;

namespace App.Domain.Entities;

public class ConjunctionEvent : BaseEntityWithMeta
{
    public Guid PrimaryObjectId { get; set; }
    public SpaceObject? PrimaryObject { get; set; }
    
    public Guid SecondaryObjectId { get; set; }
    public SpaceObject? SecondaryObject { get; set; }
    
    public double TimeOfClosestApproach { get; set; }
    public double MissDistance { get; set; }
    public double CollisionProbability { get; set; }
    public EConjunctionEventStatus Status { get; set; }

    public Guid? RiskAssessmentId { get; set; }
    public RiskAssessment? RiskAssessment { get; set; }
    
    public ICollection<Alert> Alerts { get; set; } = [];
    
    public double Threshold { get; private set; }

    public bool IsThreat => MissDistance < Threshold;

    public ConjunctionEvent(
        Guid primaryObjectId,
        Guid secondaryObjectId,
        double timeOfClosestApproach,
        double missDistance,
        double threshold)
    {
        PrimaryObjectId        = primaryObjectId;
        SecondaryObjectId      = secondaryObjectId;
        TimeOfClosestApproach  = timeOfClosestApproach;
        MissDistance           = missDistance;
        Threshold              = threshold;
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
    
    
    public void MarkAsResolved()
    {
        if (Status == EConjunctionEventStatus.AlreadyOccurred)
            throw new InvalidOperationException("Cannot resolve past events");

        Status = EConjunctionEventStatus.Resolved;
    }
    
}
