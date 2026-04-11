using System.ComponentModel.DataAnnotations;
using Base.Domain;

namespace App.Domain.Entities;

public class RiskAssessment : BaseEntityWithMeta
{
    public double RiskScore { get; set; }

    [MaxLength(100)]
    public string ModelVersion { get; set; } = string.Empty;

    public DateTime CalculatedAt { get; set; }

    public Guid SatelliteId { get; set; }
    public required Satellite Satellite { get; set; }

    public string RiskLevel => RiskScore switch
    {
        > 0.7 => "High",
        > 0.4 => "Medium",
        _     => "Low"
    };

    public void UpdateScore(double score)
    {
        if (score is < 0 or > 1)
            throw new ArgumentException("Risk score must be between 0 and 1");

        RiskScore = score;
        CalculatedAt = DateTime.UtcNow;
    }
}
