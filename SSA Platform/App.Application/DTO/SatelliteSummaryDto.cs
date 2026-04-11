using App.Domain.Entities.Enums;

namespace App.Application.DTO;

public class SatelliteSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NoradId { get; init; }
    public DateTime LaunchDate { get; init; }
    public ESatelliteStatus Status { get; init; }
    public string? RiskLevel { get; init; }
    public OrbitDto? Orbit { get; init; }
}
