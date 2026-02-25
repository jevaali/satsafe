using Base.Domain;

namespace App.Domain;

public class Orbit : BaseEntity
{
    public double SemiMajorAxis { get; set; }
    public double Eccentricity { get; set; }
    public double Inclination { get; set; }
    public double RAAN { get; set; }
    public double ArgumentOfPerigee { get; set; }
    public double MeanAnomaly { get; set; }
    public DateTime Epoch { get; set; }
    
    public Guid? SpaceObjectId { get; set; }
    public SpaceObject? SpaceObject { get; set; }
}
