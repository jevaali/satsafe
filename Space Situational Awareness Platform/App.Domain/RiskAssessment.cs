using Base.Domain;

namespace App.Domain;

public class RiskAssessment : BaseEntity
{
    public double RiskScore { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    
    public Guid ConjunctionEventId { get; set; }
    public ConjunctionEvent ConjunctionEvent { get; set; } = default!;
}
