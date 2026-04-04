using App.Domain.ValueObjects;

namespace App.Infrastructure.Seeding.dto;

public class SatelliteSeedDto
{
    public int NoradId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OperatorName { get; set; } = string.Empty;
    public DateTime LaunchDate { get; set; }
    public OrbitParameters? OrbitParameters { get; set; }
}