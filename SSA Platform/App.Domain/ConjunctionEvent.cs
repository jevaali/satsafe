using App.Domain.Enums;

namespace App.Domain;
using Base.Domain;

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

    public Guid RiskAssessmentId { get; set; }
    public RiskAssessment? RiskAssessment { get; set; }
    
    public ICollection<Alert> Alerts { get; set; } = [];
}
