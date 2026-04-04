using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;

namespace App.Infrastructure.Seeding.dto;

public class DebrisSeedDto
{
    public int NoradId { get; set; }
    public float SizeEstimate { get; set; }
    public string RiskCategory { get; set; }
    public OrbitParameters? OrbitParameters { get; set; }
}