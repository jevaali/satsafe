using Base.Domain;

namespace App.Domain;

public class Orbit : BaseEntityWithMeta
{
    public double SemiMajorAxis { get; set; }
    public double Eccentricity { get; set; }
    public double Inclination { get; set; }
    public double RAAN { get; set; }
    public double ArgumentOfPerigee { get; set; }
    public double MeanAnomaly { get; set; }
    public DateTime Epoch { get; set; }
    
    public Guid SpaceObjectId { get; set; }
    public required SpaceObject SpaceObject { get; set; }
    
    public void UpdateParameters(
        double semiMajorAxis,
        double eccentricity,
        double inclination)
    {
        if (semiMajorAxis <= 0)
            throw new ArgumentException("Semi-major axis must be positive");

        if (eccentricity < 0 || eccentricity >= 1)
            throw new ArgumentException("Eccentricity must be between 0 and 1");

        SemiMajorAxis = semiMajorAxis;
        Eccentricity = eccentricity;
        Inclination = inclination;
    }
}
