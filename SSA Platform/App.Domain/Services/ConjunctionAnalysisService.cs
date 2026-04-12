using App.Domain.Entities;

namespace App.Domain.Services;

/// <summary>
/// Screens a catalogue of space objects for conjunction events — close approaches
/// between any two objects within a future time window.
///
/// Algorithm overview (two-pass propagation):
///   1. Coarse pass  — step through the window at <c>timeStepSeconds</c> (default 60 s)
///      to locate the approximate time of closest approach (TCA).
///   2. Fine pass    — re-scan only the ±1 coarse-step bracket around that TCA at
///      1-second resolution to pin down the actual miss distance.
///
/// Positions are computed by <see cref="OrbitalMechanics.GetPositionAtTime"/>, which
/// propagates each object's Keplerian orbital elements forward in time and returns
/// an ECI (Earth-Centred Inertial) Cartesian coordinate in metres.
/// </summary>
public class ConjunctionAnalysisService
{
    // These defaults match the values expected by the use case; they are intentionally
    // conservative so that accidental callers without explicit parameters still get
    // sensible behaviour.
    private const double DefaultThresholdMeters  = 5_000;   // 5 km — typical SSA alert radius
    private const double DefaultTimeStepSeconds  = 60;      // 1-minute coarse step
    private const double DefaultWindowSeconds    = 86_400;  // 24-hour look-ahead window

    /// <summary>
    /// Evaluates every unique ordered pair from <paramref name="spaceObjects"/> and
    /// returns a <see cref="ConjunctionEvent"/> for each pair whose miss distance falls
    /// within <paramref name="thresholdMeters"/>.
    ///
    /// Complexity is O(n²) in the number of objects — acceptable for catalogue sizes in
    /// the hundreds; for larger catalogues a spatial index (e.g. bounding-box pre-filter)
    /// would be needed before calling this method.
    /// </summary>
    /// <param name="spaceObjects">All catalogued objects (satellites and debris) that
    ///   carry a valid <c>Orbit.Parameters</c>. Objects without orbit data are silently
    ///   skipped inside <see cref="FindClosestApproach"/>.</param>
    /// <param name="thresholdMeters">Maximum miss distance (in metres) that qualifies as
    ///   a conjunction. Pairs farther apart than this at their closest point are ignored.</param>
    /// <param name="windowSeconds">How far ahead to propagate, in seconds (default 24 h).</param>
    /// <param name="timeStepSeconds">Coarse sampling interval for the first pass (default 60 s).
    ///   Smaller values are more accurate but increase runtime proportionally.</param>
    public IEnumerable<ConjunctionEvent> ScreenAll(
        IReadOnlyList<SpaceObject> spaceObjects,
        double thresholdMeters = DefaultThresholdMeters * 1000,
        double windowSeconds   = DefaultWindowSeconds,
        double timeStepSeconds = DefaultTimeStepSeconds)
    {
        var results = new List<ConjunctionEvent>();

        // Iterate over each unique unordered pair (i, j) with i < j to avoid
        // screening the same pair twice (A→B and B→A produce the same event).
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

    /// <summary>
    /// Determines whether two specific objects come within <paramref name="threshold"/>
    /// metres of each other during the propagation window, and if so returns the
    /// corresponding <see cref="ConjunctionEvent"/>.
    ///
    /// Two-pass strategy:
    /// <list type="number">
    ///   <item><description>
    ///     <b>Coarse pass</b> — sample at <paramref name="timeStepSeconds"/> intervals
    ///     over the full window to find the rough TCA.  If even the coarse minimum
    ///     exceeds 10× the threshold the pair is discarded immediately — they are nowhere
    ///     near each other and the fine pass would be wasted effort.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Fine pass</b> — resample the single coarse-step bracket [TCA−step, TCA+step]
    ///     at 1-second resolution.  This narrows the miss-distance estimate without having
    ///     to run 1-second steps over the entire 24-hour window (which would be 86 400
    ///     propagations per pair instead of ~1 500).
    ///   </description></item>
    /// </list>
    /// </summary>
    private static ConjunctionEvent? FindClosestApproach(
        SpaceObject primary,
        SpaceObject secondary,
        double threshold,
        double windowSeconds,
        double timeStepSeconds)
    {
        var primaryOrbit   = primary.Orbit?.Parameters;
        var secondaryOrbit = secondary.Orbit?.Parameters;

        // Skip objects that have not been assigned orbital elements yet.
        if (primaryOrbit is null || secondaryOrbit is null)
            return null;

        double minDistance = double.MaxValue;
        double tca         = 0; // seconds from now at which the minimum occurred

        // --- Pass 1: coarse scan ---
        // Walk through the prediction window in coarse steps, propagating both objects
        // to the same epoch t and measuring their Euclidean separation in ECI space.
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

        // Early rejection: if the coarse minimum is more than 10× the threshold the
        // objects are far apart throughout the entire window.  The factor of 10 gives
        // enough headroom that the fine pass cannot possibly close the gap to threshold.
        if (minDistance > threshold * 10)
            return null;

        // --- Pass 2: fine-grained refinement around the coarse TCA ---
        // Clamp the bracket to [0, windowSeconds] so we don't propagate outside the
        // requested prediction window.
        double tMin = Math.Max(0, tca - timeStepSeconds);
        double tMax = Math.Min(windowSeconds, tca + timeStepSeconds);

        // Resample at 1-second steps — fine enough for operational conjunction analysis
        // where miss distances are measured in hundreds of metres.
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

        // Final threshold check: discard if the refined miss distance still exceeds
        // the configured limit.
        if (minDistance > threshold)
            return null;

        // The absolute TCA epoch is "now + tca seconds".  Both the miss distance and the
        // threshold are stored on the event so downstream code can judge relative severity
        // without re-running the screening.
        return new ConjunctionEvent(
            primary.Id, secondary.Id, DateTime.UtcNow.AddSeconds(tca), minDistance, threshold);
    }

    /// <summary>
    /// Euclidean distance between two ECI position vectors (in metres).
    /// </summary>
    private static double Distance(
        (double X, double Y, double Z) a,
        (double X, double Y, double Z) b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
