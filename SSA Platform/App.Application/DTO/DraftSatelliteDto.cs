namespace App.Application.DTO;

public class DraftSatelliteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public double SemiMajorAxis { get; set; }
    public double Eccentricity { get; set; }
    public double Inclination { get; set; }
    public double RAAN { get; set; }
    public double ArgumentOfPerigee { get; set; }
    public double MeanAnomaly { get; set; }

    public double AltitudeKm => SemiMajorAxis - 6371;
    public string OrbitType => AltitudeKm switch
    {
        < 2000   => "LEO",
        < 35786  => "MEO",
        < 35887  => "GEO",
        _        => "HEO"
    };
}
