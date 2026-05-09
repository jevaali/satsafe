using System.Diagnostics.CodeAnalysis;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Entities;

public class DebrisObjectTests
{
    private static OrbitParameters ValidOrbit() =>
        new OrbitParameters(
            semiMajorAxis:     6778,
            eccentricity:      0.001,
            inclination:       51.6,
            RAAN:              0,
            argumentOfPerigee: 0,
            meanAnomaly:       0);
    
    [Fact]
    public void Create_WithValidArguments_ReturnsSatelliteWithExpectedRiskCategory()
    {
        var debris = DebrisObject.Create(0.5f, EDebrisObjectRiskCategory.Medium);

        debris.RiskCategory.Should().Be(EDebrisObjectRiskCategory.Medium);
    }

    [Fact]
    public void Create_WithValidArguments_AssignsNewId()
    {
        var d1 = DebrisObject.Create(0.5f, EDebrisObjectRiskCategory.Low);
        var d2 = DebrisObject.Create(0.5f, EDebrisObjectRiskCategory.Low);

        d1.Id.Should().NotBe(d2.Id);
    }

    [Fact]
    public void Create_WithValidArguments_SetsCreatedByToSystem()
    {
        var debris = DebrisObject.Create(1.0f, EDebrisObjectRiskCategory.High);

        debris.CreatedBy.Should().Be("system");
    }

    // ── CreateFromExternalData ────────────────────────────────────────────────

    [Theory]
    [InlineData("Low",    EDebrisObjectRiskCategory.Low)]
    [InlineData("Medium", EDebrisObjectRiskCategory.Medium)]
    [InlineData("High",   EDebrisObjectRiskCategory.High)]
    [InlineData("low",    EDebrisObjectRiskCategory.Low)]   // case-insensitive
    public void CreateFromExternalData_WithRiskCategoryString_ParsesCorrectly(
        string rawCategory, EDebrisObjectRiskCategory expected)
    {
        var debris = DebrisObject.CreateFromExternalData(99999, 2.0f, rawCategory, null);

        debris.RiskCategory.Should().Be(expected);
    }

    [Fact]
    public void CreateFromExternalData_WithOrbitParameters_SetsOrbit()
    {
        var debris = DebrisObject.CreateFromExternalData(99999, 2.0f, "Low", ValidOrbit());

        debris.Orbit.Should().NotBeNull();
    }

    [Fact]
    public void CreateFromExternalData_WithNullOrbitParameters_LeavesOrbitNull()
    {
        var debris = DebrisObject.CreateFromExternalData(99999, 2.0f, "Low", null);

        debris.Orbit.Should().BeNull();
    }

    [Fact]
    public void CreateFromExternalData_WithNoradId_SetsNoradId()
    {
        var debris = DebrisObject.CreateFromExternalData(12345, 2.0f, "Low", null);

        debris.NoradId.Should().Be(12345);
    }

    [Fact]
    public void CreateFromExternalData_WithInvalidRiskCategory_ThrowsArgumentException()
    {
        Action act = () => DebrisObject.CreateFromExternalData(1, 1.0f, "INVALID", null);

        act.Should().Throw<ArgumentException>();
    }
}

public class SpaceObjectTests
{
    // [SetsRequiredMembers] tells the compiler this constructor fulfils
    // all required members, allowing Orbit = null inside the body.
    private class TestSpaceObject : SpaceObject
    {
        [SetsRequiredMembers]
        public TestSpaceObject()
        {
            Id        = Guid.NewGuid();
            Orbit     = null;
            CreatedBy = "system";
            CreatedAt = DateTime.UtcNow;
        }
    }

    private static OrbitParameters ValidOrbit() =>
        new OrbitParameters(
            semiMajorAxis:     6778,
            eccentricity:      0.001,
            inclination:       51.6,
            RAAN:              0,
            argumentOfPerigee: 0,
            meanAnomaly:       0);

    private static OrbitParameters OrbitWithInclination(double inclination) =>
        new OrbitParameters(
            semiMajorAxis:     6778,
            eccentricity:      0.001,
            inclination:       inclination,
            RAAN:              0,
            argumentOfPerigee: 0,
            meanAnomaly:       0);

    // ── SetOrbit ──────────────────────────────────────────────────────────────

    [Fact]
    public void SetOrbit_WhenOrbitIsNull_CreatesNewOrbit()
    {
        var obj = new TestSpaceObject();

        obj.SetOrbit(ValidOrbit());

        obj.Orbit.Should().NotBeNull();
    }

    [Fact]
    public void SetOrbit_WhenOrbitExists_UpdatesExistingOrbit()
    {
        var obj = new TestSpaceObject();
        obj.SetOrbit(ValidOrbit());
        var originalOrbit = obj.Orbit;

        obj.SetOrbit(OrbitWithInclination(90));

        // same object reference updated, not replaced
        obj.Orbit.Should().BeSameAs(originalOrbit);
    }

    [Fact]
    public void SetOrbit_WithNullParameters_ThrowsArgumentNullException()
    {
        var obj = new TestSpaceObject();

        Action act = () => obj.SetOrbit(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── AddTleRecord ──────────────────────────────────────────────────────────

    [Fact]
    public void AddTleRecord_WithRecentEpoch_AddsTleRecord()
    {
        var obj    = new TestSpaceObject();
        var record = new TleRecord
        {
            SpaceObject   = obj,
            SpaceObjectId = obj.Id,
            Epoch         = DateTime.UtcNow.AddDays(-1),
            RawLine1      = string.Empty,
            RawLine2      = string.Empty
        };

        obj.AddTleRecord(record);

        obj.TleRecords.Should().ContainSingle();
    }

    [Fact]
    public void AddTleRecord_WithTooOldEpoch_ThrowsInvalidOperationException()
    {
        var obj    = new TestSpaceObject();
        var record = new TleRecord
        {
            SpaceObject   = obj,
            SpaceObjectId = obj.Id,
            Epoch         = DateTime.UtcNow.AddDays(-31),
            RawLine1      = string.Empty,
            RawLine2      = string.Empty
        };

        Action act = () => obj.AddTleRecord(record);

        act.Should().Throw<InvalidOperationException>();
    }
}