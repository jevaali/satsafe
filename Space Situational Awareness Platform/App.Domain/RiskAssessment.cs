using Base.Domain;

namespace App.Domain;

public class RiskAssessment : BaseEntity
{
    // Foreign key
    public Guid ConjunctionEventId { get; set; }

    public double RiskScore { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }

    // Navigation properties
    public ConjunctionEvent? ConjunctionEvent { get; set; }
}
