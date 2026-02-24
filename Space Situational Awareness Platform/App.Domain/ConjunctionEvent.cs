namespace App.Domain;
using Base.Domain;

public class ConjunctionEvent : BaseEntity
{
    // Foreign keys
    public Guid PrimaryObjectId { get; set; }
    public Guid SecondaryObjectId { get; set; }

    public DateTime TimeOfClosestApproach { get; set; }
    public double MissDistance { get; set; }
    public double CollisionProbability { get; set; }
    public string Status { get; set; } = string.Empty;

    // Navigation properties
    public RiskAssessment? RiskAssessment { get; set; }
    public List<Alert> Alerts { get; set; } = new();
}
