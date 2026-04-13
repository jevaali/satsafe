namespace App.Application.Features.Satellites.DTOs;

public class CelesTrakTleDto
{
    public string Name { get; init; } = string.Empty;
    public int NoradId { get; init; }
    public string Line1 { get; init; } = string.Empty;
    public string Line2 { get; init; } = string.Empty;
    public DateTime Epoch { get; init; }
    public double Inclination { get; init; }
    public double RAAN { get; init; }
    public double Eccentricity { get; init; }
    public double ArgumentOfPerigee { get; init; }
    public double MeanAnomaly { get; init; }
    public double MeanMotion { get; init; }
    public double BSTar { get; init; }
    public double FirstDerivative { get; init; }
    public double SecondDerivative { get; init; }
    public int RevolutionNumber { get; init; }
}
