using App.Domain.ValueObjects;

namespace App.Domain.Services;

public static class OrbitalMechanics
{
    private const double GM = 3.986004418e14;
    
    public static (double X, double Y, double Z) GetPositionAtTime(
        OrbitParameters orbit, double deltaTimeSeconds)
    {
        // 1. Mean motion (rad/s)
        double n = Math.Sqrt(GM / Math.Pow(orbit.SemiMajorAxis, 3));

        // 2. Propagate mean anomaly
        double M = DegreesToRadians(orbit.MeanAnomaly) + n * deltaTimeSeconds;
        M = NormalizeAngle(M);

        // 3. Solve Kepler's equation: M = E - e*sin(E)
        double E = SolveKeplerEquation(M, orbit.Eccentricity);

        // 4. True anomaly
        double nu = TrueAnomaly(E, orbit.Eccentricity);

        // 5. Distance from focus
        double r = orbit.SemiMajorAxis * (1 - orbit.Eccentricity * Math.Cos(E));

        // 6. Position in orbital plane (perifocal frame)
        double xOrbital = r * Math.Cos(nu);
        double yOrbital = r * Math.Sin(nu);

        // 7. Rotate to ECI using Euler angles (Ω, i, ω)
        double omega = DegreesToRadians(orbit.ArgumentOfPerigee);
        double raan  = DegreesToRadians(orbit.RAAN);
        double inc   = DegreesToRadians(orbit.Inclination);

        double cosO = Math.Cos(raan),  sinO = Math.Sin(raan);
        double cosW = Math.Cos(omega), sinW = Math.Sin(omega);
        double cosI = Math.Cos(inc),   sinI = Math.Sin(inc);

        // Rotation matrix R = Rz(-Ω) * Rx(-i) * Rz(-ω) applied to perifocal vector
        double x = (cosO * cosW - sinO * sinW * cosI) * xOrbital
                 + (-cosO * sinW - sinO * cosW * cosI) * yOrbital;

        double y = (sinO * cosW + cosO * sinW * cosI) * xOrbital
                 + (-sinO * sinW + cosO * cosW * cosI) * yOrbital;

        double z = (sinW * sinI) * xOrbital
                 + (cosW * sinI) * yOrbital;

        return (x, y, z);
    }
    
    private static double SolveKeplerEquation(double M, double eccentricity, int maxIter = 100)
    {
        double E = M; 
        for (int i = 0; i < maxIter; i++)
        {
            double dE = (M - E + eccentricity * Math.Sin(E))
                      / (1 - eccentricity * Math.Cos(E));
            E += dE;
            if (Math.Abs(dE) < 1e-12) break;
        }
        return E;
    }

    private static double TrueAnomaly(double E, double e)
    {
        double sinNu = Math.Sqrt(1 - e * e) * Math.Sin(E) / (1 - e * Math.Cos(E));
        double cosNu = (Math.Cos(E) - e) / (1 - e * Math.Cos(E));
        return Math.Atan2(sinNu, cosNu);
    }

    private static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;
    private static double NormalizeAngle(double rad) =>
        rad - 2 * Math.PI * Math.Floor(rad / (2 * Math.PI));
}