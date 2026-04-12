namespace App.Application.Common.DTOs;

public class OrbitDto
{
    public double SemiMajorAxis { get; init; }
    public double Eccentricity { get; init; }
    public double Inclination { get; init; }
    public double RAAN { get; init; }
    public double ArgumentOfPerigee { get; init; }
    public double MeanAnomaly { get; init; }
}
