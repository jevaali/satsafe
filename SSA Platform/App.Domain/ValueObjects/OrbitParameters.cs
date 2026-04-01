namespace App.Domain.ValueObjects;

public class OrbitParameters
{
    public double SemiMajorAxis { get; }
    public double Eccentricity { get; }
    public double Inclination { get; }
    public double RAAN { get; }
    public double ArgumentOfPerigee { get; }
    public double MeanAnomaly { get; }

    public OrbitParameters(
        double semiMajorAxis,
        double eccentricity,
        double inclination,
        double RAAN,
        double argumentOfPerigee,
        double meanAnomaly)
    {
        if (semiMajorAxis <= 0)
            throw new ArgumentOutOfRangeException(nameof(semiMajorAxis));
        if (eccentricity is < 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(eccentricity));

        SemiMajorAxis = semiMajorAxis;
        Eccentricity = eccentricity;
        Inclination = inclination;
        RAAN = RAAN;
        ArgumentOfPerigee = argumentOfPerigee;
        MeanAnomaly = meanAnomaly;
    }

    public double CalculateOrbitalPeriod()
    {
        const double G = 6.67430e-11; // gravitational constant
        const double M = 5.972e24;    // Earth's mass in kg
        return 2 * Math.PI * Math.Sqrt(Math.Pow(SemiMajorAxis, 3) / (G * M));
    }

    public override bool Equals(object? obj) =>
        obj is OrbitParameters p &&
        SemiMajorAxis == p.SemiMajorAxis &&
        Eccentricity == p.Eccentricity &&
        Inclination == p.Inclination &&
        RAAN == p.RAAN &&
        ArgumentOfPerigee == p.ArgumentOfPerigee &&
        MeanAnomaly == p.MeanAnomaly;

    public override int GetHashCode() =>
        HashCode.Combine(SemiMajorAxis, Eccentricity, Inclination, RAAN, ArgumentOfPerigee, MeanAnomaly);
}