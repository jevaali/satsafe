namespace App.Application.Features.Satellites.DTOs;

public class RiskAssessmentDto
{
    public double RiskScore { get; init; }
    public string RiskLevel { get; init; } = string.Empty;
    public string ModelVersion { get; init; } = string.Empty;
    public DateTime CalculatedAt { get; init; }
}
