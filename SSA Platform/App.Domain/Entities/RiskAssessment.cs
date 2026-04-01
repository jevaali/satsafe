using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class RiskAssessment : BaseEntityWithMeta
{
    public double RiskScore { get; set; }
   
    [MaxLength(100)]
    public string ModelVersion { get; set; } = string.Empty;
    public Guid ConjunctionEventId { get; set; }
    public required ConjunctionEvent ConjunctionEvent { get; set; }
    
    public void UpdateRiskScore(double score)
    {
        if (score < 0)
            throw new ArgumentException("Risk score cannot be negative");

        RiskScore = score;
    }
}
