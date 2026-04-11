using App.Domain.ValueObjects;

namespace App.Domain.Services;

/// <summary>
/// Calculates an orbit-based risk score for a satellite by counting how many
/// other space objects share an overlapping altitude band and similar inclination.
/// The more crowded the orbital neighbourhood, the higher the risk.
/// </summary>
public class SatelliteRiskScoringService
{
    private const double EarthRadiusMeters = 6_371_000;

    // Two orbits are considered co-planar enough to be risky when their
    // inclinations differ by less than this value.
    private const double InclinationThresholdDegrees = 15.0;

    // Score saturates at this many overlapping objects (= score 1.0).
    private const double NormalizationFactor = 50.0;

    public double Calculate(
        OrbitParameters satellite,
        IReadOnlyList<OrbitParameters> others)
    {
        ArgumentNullException.ThrowIfNull(satellite);

        double satPerigee = satellite.SemiMajorAxis * (1 - satellite.Eccentricity) - EarthRadiusMeters;
        double satApogee  = satellite.SemiMajorAxis * (1 + satellite.Eccentricity) - EarthRadiusMeters;

        int count = 0;
        foreach (var other in others)
        {
            if (other is null) continue;

            double otherPerigee = other.SemiMajorAxis * (1 - other.Eccentricity) - EarthRadiusMeters;
            double otherApogee  = other.SemiMajorAxis * (1 + other.Eccentricity) - EarthRadiusMeters;

            bool altitudeOverlap      = satPerigee < otherApogee && otherPerigee < satApogee;
            bool inclinationProximity = Math.Abs(satellite.Inclination - other.Inclination) < InclinationThresholdDegrees;

            if (altitudeOverlap && inclinationProximity)
                count++;
        }

        return Math.Min(1.0, count / NormalizationFactor);
    }
}
