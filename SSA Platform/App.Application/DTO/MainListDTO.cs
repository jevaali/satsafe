namespace App.Application.DTO;

public class MainListDTO
{
    public List<MainListSatDTO> Satellites { get; set; } = [];
    public List<MainListDebrisDTO> Debris { get; set; } = [];
    
}

public class MainListSatDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Operator { get; set; } = default!;
    public bool HasOrbit { get; set; }
    public double? SemiMajorAxis { get; set; }
    public double? Eccentricity { get; set; }
    public double? Inclination { get; set; }
    public double? Raan { get; set; }
    public double? ArgumentOfPerigee { get; set; }
    public double? MeanAnomaly { get; set; }
}

public class MainListDebrisDTO
{
    public Guid Id { get; set; }
    public int NoradId { get; set; }
    public float SizeEstimate { get; set; }
    public bool HasOrbit { get; set; }
    public double? SemiMajorAxis { get; set; }
    public double? Eccentricity { get; set; }
    public double? Inclination { get; set; }
    public double? Raan { get; set; }
    public double? ArgumentOfPerigee { get; set; }
    public double? MeanAnomaly { get; set; }
}