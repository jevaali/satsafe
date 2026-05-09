using System.Diagnostics.CodeAnalysis;
using App.Domain.Entities;
using App.Domain.Services;
using App.Domain.ValueObjects;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Services;

public class ConjunctionAnalysisServiceTests
{
    private readonly ConjunctionAnalysisService _sut = new();

    // ── Test doubles ──────────────────────────────────────────────────────────

    private class TestOrbit : Orbit
    {
        [SetsRequiredMembers]
        public TestOrbit(OrbitParameters parameters, Guid spaceObjectId)
            : base(parameters)
        {
            SpaceObject   = null!;   // navigation property — not needed for screening logic
            SpaceObjectId = spaceObjectId;
        }
    }

    private class TestSpaceObject : SpaceObject
    {
        [SetsRequiredMembers]
        public TestSpaceObject(OrbitParameters? orbitParams = null)
        {
            Id        = Guid.NewGuid();
            CreatedBy = "system";
            CreatedAt = DateTime.UtcNow;
            Orbit     = orbitParams is null ? null : new TestOrbit(orbitParams, Id);
        }
    }

    // ── Orbit helpers ─────────────────────────────────────────────────────────

    // Two near-identical ISS-like orbits — nearly co-located so they converge
    // within any generous threshold during the first few seconds of propagation.
    private static OrbitParameters NearOrbitA() =>
        new OrbitParameters(
            semiMajorAxis:     6_778_000,
            eccentricity:      0.0,
            inclination:       51.6,
            RAAN:              0.0,
            argumentOfPerigee: 0.0,
            meanAnomaly:       0.0);

    private static OrbitParameters NearOrbitB() =>
        new OrbitParameters(
            semiMajorAxis:     6_778_000,
            eccentricity:      0.0,
            inclination:       51.6,
            RAAN:              0.0,
            argumentOfPerigee: 0.0,
            meanAnomaly:       0.001);   // tiny offset — nearly co-located

    // Orbit in a completely different plane and altitude — never comes close.
    private static OrbitParameters FarOrbit() =>
        new OrbitParameters(
            semiMajorAxis:     7_371_000,  // 1000 km higher
            eccentricity:      0.0,
            inclination:       98.0,
            RAAN:              0.0,
            argumentOfPerigee: 0.0,
            meanAnomaly:       0.0);

    // ── Empty / trivial inputs ────────────────────────────────────────────────

    [Fact]
    public void ScreenAll_WithEmptyList_ReturnsNoEvents()
    {
        var result = _sut.ScreenAll([]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ScreenAll_WithSingleObject_ReturnsNoEvents()
    {
        var result = _sut.ScreenAll([new TestSpaceObject(NearOrbitA())]);

        result.Should().BeEmpty();
    }

    // ── Objects without orbit are silently skipped ────────────────────────────

    [Fact]
    public void ScreenAll_WhenOneObjectHasNoOrbit_DoesNotThrow()
    {
        var withOrbit    = new TestSpaceObject(NearOrbitA());
        var withoutOrbit = new TestSpaceObject(orbitParams: null);

        Action act = () => _sut.ScreenAll([withOrbit, withoutOrbit]);

        act.Should().NotThrow();
    }

    [Fact]
    public void ScreenAll_WhenBothObjectsHaveNoOrbit_ReturnsNoEvents()
    {
        var a = new TestSpaceObject(orbitParams: null);
        var b = new TestSpaceObject(orbitParams: null);

        var result = _sut.ScreenAll([a, b]);

        result.Should().BeEmpty();
    }

    // ── Close objects produce a conjunction event ─────────────────────────────

    [Fact]
    public void ScreenAll_WithTwoNearlyColocatedObjects_ReturnsConjunctionEvent()
    {
        var a = new TestSpaceObject(NearOrbitA());
        var b = new TestSpaceObject(NearOrbitB());

        var result = _sut.ScreenAll([a, b], thresholdMeters: 50_000);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public void ScreenAll_WithTwoNearlyColocatedObjects_EventContainsBothObjectIds()
    {
        var a = new TestSpaceObject(NearOrbitA());
        var b = new TestSpaceObject(NearOrbitB());

        var result = _sut.ScreenAll([a, b], thresholdMeters: 50_000).ToList();

        result.Should().ContainSingle();
        var evt = result[0];
        new[] { evt.PrimaryObjectId, evt.SecondaryObjectId }
            .Should().Contain(a.Id).And.Contain(b.Id);
    }

    // ── Far-apart objects produce no event ───────────────────────────────────

    [Fact]
    public void ScreenAll_WithTwoWellSeparatedObjects_ReturnsNoEvents()
    {
        var a = new TestSpaceObject(NearOrbitA());
        var b = new TestSpaceObject(FarOrbit());

        var result = _sut.ScreenAll([a, b], thresholdMeters: 5_000);

        result.Should().BeEmpty();
    }

    // ── Pair-count correctness ────────────────────────────────────────────────
    // n objects → n*(n-1)/2 unique pairs → result count can never exceed that.

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 3)]
    [InlineData(4, 6)]
    public void ScreenAll_ResultCount_NeverExceedsUniqueUnorderedPairs(
        int objectCount, int maxPairs)
    {
        var objects = Enumerable
            .Range(0, objectCount)
            .Select(_ => (SpaceObject)new TestSpaceObject(FarOrbit()))
            .ToList();

        var result = _sut.ScreenAll(objects);

        result.Count().Should().BeLessThanOrEqualTo(maxPairs);
    }

    [Fact]
    public void ScreenAll_WithThreeNearObjects_ReturnsAtMostThreeEvents()
    {
        var objects = new SpaceObject[]
        {
            new TestSpaceObject(NearOrbitA()),
            new TestSpaceObject(NearOrbitB()),
            new TestSpaceObject(NearOrbitA())
        };

        var result = _sut.ScreenAll(objects, thresholdMeters: 50_000);

        result.Count().Should().BeLessThanOrEqualTo(3);
    }
}