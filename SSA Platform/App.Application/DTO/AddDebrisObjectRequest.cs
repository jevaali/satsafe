using App.Domain.Entities.Enums;

namespace App.Application.DTO;

public class AddDebrisObjectRequest
{
    public float SizeEstimate { get; set; }
    public EDebrisObjectRiskCategory RiskCategory { get; set; }

    public double SemiMajorAxis { get; set; }
    public double Eccentricity { get; set; }
    public double Inclination { get; set; }
    public double RAAN { get; set; }
    public double ArgumentOfPerigee { get; set; }
    public double MeanAnomaly { get; set; }
}
