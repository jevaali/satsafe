using App.Domain.Services;
using App.Domain.ValueObjects;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Services;

public class OrbitalMechanicsTests
{
    // ISS-like circular orbit at ~400 km altitude
    // Circular orbit (e=0) makes hand-verification straightforward:
    // the satellite traces a perfect circle, so |position| == SemiMajorAxis at all times.
    private static OrbitParameters CircularOrbit() =>
        new OrbitParameters(
            semiMajorAxis:     6_778_000,   // 400 km above Earth's surface (metres)
            eccentricity:      0.0,
            inclination:       0.0,         // equatorial — Z should stay ~0
            RAAN:              0.0,
            argumentOfPerigee: 0.0,
            meanAnomaly:       0.0);

    private static OrbitParameters InclinedCircularOrbit(double inclination) =>
        new OrbitParameters(
            semiMajorAxis:     6_778_000,
            eccentricity:      0.0,
            inclination:       inclination,
            RAAN:              0.0,
            argumentOfPerigee: 0.0,
            meanAnomaly:       0.0);

    // ── Radius conservation ───────────────────────────────────────────────────
    // For any Keplerian orbit the distance from the focus equals
    // r = a(1 - e·cos(E)). For a circular orbit (e=0) this simplifies to r = a
    // at every point — a good invariant to check across different propagation times.

    [Theory]
    [InlineData(0)]
    [InlineData(1000)]
    [InlineData(3600)]
    [InlineData(86_400)]
    public void GetPositionAtTime_CircularOrbit_RadiusEqualsSetMajorAxisAtAllTimes(double deltaSeconds)
    {
        var orbit = CircularOrbit();

        var (x, y, z) = OrbitalMechanics.GetPositionAtTime(orbit, deltaSeconds);
        double radius = Math.Sqrt(x * x + y * y + z * z);

        radius.Should().BeApproximately(orbit.SemiMajorAxis, precision: 1.0); // within 1 metre
    }

    // ── Equatorial orbit: Z ≈ 0 ───────────────────────────────────────────────
    // An equatorial orbit (i=0) lies entirely in the XY plane — Z must be zero.

    [Theory]
    [InlineData(0)]
    [InlineData(1800)]
    [InlineData(5400)]
    public void GetPositionAtTime_EquatorialOrbit_ZComponentIsNearZero(double deltaSeconds)
    {
        var orbit = CircularOrbit(); // inclination = 0

        var (_, _, z) = OrbitalMechanics.GetPositionAtTime(orbit, deltaSeconds);

        z.Should().BeApproximately(0.0, precision: 1.0);
    }

    // ── Polar orbit: Z spans full range ──────────────────────────────────────
    // For a polar orbit (i=90°) the satellite passes directly over both poles,
    // so |Z| should reach the full orbital radius within one period.
    // Orbital period for ISS-like orbit ≈ 5560 s — test at quarter period.

    [Fact]
    public void GetPositionAtTime_PolarOrbit_ZComponentIsNonZero()
    {
        var orbit = InclinedCircularOrbit(inclination: 90.0);

        // At t=0 the satellite starts at meanAnomaly=0 (ascending node).
        // After a quarter period it should be near the pole — Z ≈ SemiMajorAxis.
        double quarterPeriod = 2 * Math.PI * Math.Sqrt(Math.Pow(orbit.SemiMajorAxis, 3) / 3.986004418e14) / 4;

        var (_, _, z) = OrbitalMechanics.GetPositionAtTime(orbit, quarterPeriod);

        Math.Abs(z).Should().BeGreaterThan(orbit.SemiMajorAxis * 0.9);
    }

    // ── One full period returns to start ─────────────────────────────────────
    // After exactly one orbital period the satellite must be back at its
    // starting position (Keplerian orbit is periodic).

    [Fact]
    public void GetPositionAtTime_AfterOneFullPeriod_PositionMatchesStart()
    {
        var orbit  = CircularOrbit();
        double period = 2 * Math.PI * Math.Sqrt(Math.Pow(orbit.SemiMajorAxis, 3) / 3.986004418e14);

        var start = OrbitalMechanics.GetPositionAtTime(orbit, 0);
        var end   = OrbitalMechanics.GetPositionAtTime(orbit, period);

        end.X.Should().BeApproximately(start.X, precision: 10.0); // within 10 m
        end.Y.Should().BeApproximately(start.Y, precision: 10.0);
        end.Z.Should().BeApproximately(start.Z, precision: 10.0);
    }

    // ── t=0 returns initial position ─────────────────────────────────────────
    // With meanAnomaly=0 and argumentOfPerigee=0 and RAAN=0 and inclination=0,
    // the satellite is at the ascending node — position should be (a, 0, 0).

    [Fact]
    public void GetPositionAtTime_AtTimeZero_ReturnsInitialPosition()
    {
        var orbit = CircularOrbit();

        var (x, y, z) = OrbitalMechanics.GetPositionAtTime(orbit, 0);

        x.Should().BeApproximately(orbit.SemiMajorAxis, precision: 1.0);
        y.Should().BeApproximately(0.0, precision: 1.0);
        z.Should().BeApproximately(0.0, precision: 1.0);
    }
}