using App.Domain.Entities;

namespace App.Domain.Services;

public class ConjunctionAnalysisService
{
    private const double DefaultThresholdMeters  = 5_000;   // 5 km
    private const double DefaultTimeStepSeconds  = 60;      // 1-minute steps
    private const double DefaultWindowSeconds    = 86_400;  // 24 hours
    
    public IEnumerable<ConjunctionEvent> ScreenAll(
        IReadOnlyList<SpaceObject> spaceObjects,
        double thresholdMeters = DefaultThresholdMeters,
        double windowSeconds   = DefaultWindowSeconds,
        double timeStepSeconds = DefaultTimeStepSeconds)
    {
        foreach (var obj in spaceObjects)
        {
            Console.WriteLine($"[Screening] Object: {obj.Id} | Orbit: {obj.Orbit?.Id} | Parameters: {obj.Orbit?.Parameters?.SemiMajorAxis}");
        }
        
        var results = new List<ConjunctionEvent>();

        for (int i = 0; i < spaceObjects.Count; i++)
        for (int j = i + 1; j < spaceObjects.Count; j++)
        {
            var evt = FindClosestApproach(
                spaceObjects[i], spaceObjects[j],
                thresholdMeters, windowSeconds, timeStepSeconds);

            if (evt is not null)
                results.Add(evt);
        }

        return results;
    }

    private static ConjunctionEvent? FindClosestApproach(
        SpaceObject primary,
        SpaceObject secondary,
        double threshold,
        double windowSeconds,
        double timeStepSeconds)
    {
        var primaryOrbit   = primary.Orbit?.Parameters;
        var secondaryOrbit = secondary.Orbit?.Parameters;

        if (primaryOrbit is null || secondaryOrbit is null)
            return null;

        double minDistance = double.MaxValue;
        double tca         = 0;

        for (double t = 0; t <= windowSeconds; t += timeStepSeconds)
        {
            double dist = Distance(
                OrbitalMechanics.GetPositionAtTime(primaryOrbit, t),
                OrbitalMechanics.GetPositionAtTime(secondaryOrbit, t));

            if (dist < minDistance)
            {
                minDistance = dist;
                tca         = t;
            }
        }

        if (minDistance > threshold * 10)
            return null;

        double tMin = Math.Max(0, tca - timeStepSeconds);
        double tMax = Math.Min(windowSeconds, tca + timeStepSeconds);

        for (double t = tMin; t <= tMax; t += 1.0)
        {
            double dist = Distance(
                OrbitalMechanics.GetPositionAtTime(primaryOrbit, t),
                OrbitalMechanics.GetPositionAtTime(secondaryOrbit, t));

            if (dist < minDistance)
            {
                minDistance = dist;
                tca         = t;
            }
        }

        if (minDistance > threshold)
            return null;

        return new ConjunctionEvent(
            primary.Id, secondary.Id, tca, minDistance, threshold);
    }

    private static double Distance(
        (double X, double Y, double Z) a,
        (double X, double Y, double Z) b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}