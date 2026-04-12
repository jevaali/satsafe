using App.Application.Common.DTOs;
using App.Domain.Entities.Enums;

namespace App.Application.Features.Satellites.DTOs;

public class SatelliteDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NoradId { get; init; }
    public DateTime LaunchDate { get; init; }
    public ESatelliteStatus Status { get; init; }
    public OrbitDto? Orbit { get; init; }
    public RiskAssessmentDto? RiskAssessment { get; init; }
}
