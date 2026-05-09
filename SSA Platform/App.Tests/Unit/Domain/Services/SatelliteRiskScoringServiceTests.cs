using App.Domain.Services;
using App.Domain.ValueObjects;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Services;

public class SatelliteRiskScoringServiceTests
{
    private readonly SatelliteRiskScoringService _sut = new();

    // Orbit at ~400 km altitude, 51.6° inclination (ISS-like)
    private static OrbitParameters Orbit(
        double semiMajorAxis = 6_778_000,
        double eccentricity  = 0.0,
        double inclination   = 51.6) =>
        new OrbitParameters(
            semiMajorAxis:     semiMajorAxis,
            eccentricity:      eccentricity,
            inclination:       inclination,
            RAAN:              0,
            argumentOfPerigee: 0,
            meanAnomaly:       0);

    // ── Empty catalogue ───────────────────────────────────────────────────────

    [Fact]
    public void Calculate_WithNoOtherObjects_ReturnsZero()
    {
        var score = _sut.Calculate(Orbit(), others: []);

        score.Should().Be(0.0);
    }

    // ── Altitude overlap logic ────────────────────────────────────────────────

    [Fact]
    public void Calculate_WithOneObjectSameAltitudeAndInclination_ReturnsNonZeroScore()
    {
        var score = _sut.Calculate(Orbit(), others: [Orbit()]);

        score.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Calculate_WithObjectAtCompletelyDifferentAltitude_ReturnsZero()
    {
        // Put the other object at ~1000 km above (no overlap with 400 km orbit)
        var highOrbit = Orbit(semiMajorAxis: 7_371_000);

        var score = _sut.Calculate(Orbit(), others: [highOrbit]);

        score.Should().Be(0.0);
    }

    // ── Inclination threshold ─────────────────────────────────────────────────

    [Fact]
    public void Calculate_WithObjectJustInsideInclinationThreshold_CountsAsRisky()
    {
        // threshold is 15°; use 14° difference — should count
        var nearInclinationOrbit = Orbit(inclination: 51.6 + 14.0);

        var score = _sut.Calculate(Orbit(), others: [nearInclinationOrbit]);

        score.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public void Calculate_WithObjectJustOutsideInclinationThreshold_IsNotCounted()
    {
        // threshold is 15°; use exactly 15° difference — should NOT count (< not <=)
        var farInclinationOrbit = Orbit(inclination: 66.6);

        var score = _sut.Calculate(Orbit(), others: [farInclinationOrbit]);

        score.Should().Be(0.0);
    }

    [Fact]
    public void Calculate_WithObjectWellOutsideInclination_IsNotCounted()
    {
        var polarOrbit = Orbit(inclination: 90.0); // 38.4° difference

        var score = _sut.Calculate(Orbit(), others: [polarOrbit]);

        score.Should().Be(0.0);
    }

    // ── Score scaling and clamping ────────────────────────────────────────────

    [Fact]
    public void Calculate_ScoreIncreasesWithMoreOverlappingObjects()
    {
        var oneNeighbour  = new List<OrbitParameters> { Orbit() };
        var fiveNeighbours = Enumerable.Repeat(Orbit(), 5).ToList();

        var scoreOne  = _sut.Calculate(Orbit(), oneNeighbour);
        var scoreFive = _sut.Calculate(Orbit(), fiveNeighbours);

        scoreFive.Should().BeGreaterThan(scoreOne);
    }

    [Fact]
    public void Calculate_WithFiftyOrMoreOverlappingObjects_ReturnsCappedScoreOfOne()
    {
        // NormalizationFactor = 50 — at 50 overlapping objects the score saturates at 1.0
        var fiftyNeighbours = Enumerable.Repeat(Orbit(), 50).ToList();

        var score = _sut.Calculate(Orbit(), fiftyNeighbours);

        score.Should().Be(1.0);
    }

    [Fact]
    public void Calculate_WithMoreThanFiftyOverlappingObjects_DoesNotExceedOne()
    {
        var hundredNeighbours = Enumerable.Repeat(Orbit(), 100).ToList();

        var score = _sut.Calculate(Orbit(), hundredNeighbours);

        score.Should().Be(1.0);
    }

    [Fact]
    public void Calculate_ScoreIsAlwaysBetweenZeroAndOne()
    {
        var neighbours = Enumerable.Repeat(Orbit(), 25).ToList();

        var score = _sut.Calculate(Orbit(), neighbours);

        score.Should().BeInRange(0.0, 1.0);
    }

    // ── Null guard ────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_WithNullSatellite_ThrowsArgumentNullException()
    {
        Action act = () => _sut.Calculate(null!, others: []);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── MemberData: mixed neighbourhood ──────────────────────────────────────

    public static IEnumerable<object[]> NeighbourhoodCases()
    {
        // (risky objects, non-risky objects, expected score > 0)
        yield return new object[] { 0,  5, false };
        yield return new object[] { 1,  0, true  };
        yield return new object[] { 3,  5, true  };
        yield return new object[] { 50, 0, true  };
    }

    [Theory]
    [MemberData(nameof(NeighbourhoodCases))]
    public void Calculate_MixedNeighbourhood_ScoreReflectsOnlyRiskyObjects(
        int riskyCount, int safeCount, bool expectNonZero)
    {
        var risky = Enumerable.Repeat(Orbit(), riskyCount);
        var safe  = Enumerable.Repeat(Orbit(semiMajorAxis: 7_371_000), safeCount); // different altitude
        var others = risky.Concat(safe).ToList();

        var score = _sut.Calculate(Orbit(), others);

        if (expectNonZero)
            score.Should().BeGreaterThan(0.0);
        else
            score.Should().Be(0.0);
    }
}