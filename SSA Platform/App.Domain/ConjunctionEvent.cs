namespace App.Domain;
using Base.Domain;

public class ConjunctionEvent : BaseEntityWithMeta
{
    public Guid PrimaryObjectId { get; set; }
    public virtual SpaceObject? PrimaryObject { get; set; }
    
    public Guid SecondaryObjectId { get; set; }
    public virtual SpaceObject? SecondaryObject { get; set; }
    
    public DateTime TimeOfClosestApproach { get; set; }
    public double MissDistance { get; set; }
    public double CollisionProbability { get; set; }
    public string Status { get; set; } = string.Empty;

    public Guid RiskAssessmentId { get; set; }
    public virtual RiskAssessment? RiskAssessment { get; set; }
    
    public virtual ICollection<Alert> Alerts { get; set; } = [];
}
