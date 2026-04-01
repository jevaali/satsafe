namespace App.Application.DTO;

public class AddSatelliteRequest
{
    public string Name { get; set; } = default!;
    public DateTime LaunchDate { get; set; }
    public Guid OperatorId { get; set; }

    public double SemiMajorAxis { get; set; }
    public double Eccentricity { get; set; }
    public double Inclination { get; set; }
    public double RAAN { get; set; }
    public double ArgumentOfPerigee { get; set; }
    public double MeanAnomaly { get; set; }
}