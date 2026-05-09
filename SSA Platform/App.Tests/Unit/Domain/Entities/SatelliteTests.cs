using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.ValueObjects;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Entities;

public class SatelliteTests
{
    private static Operator MakeOperator() =>
        new Operator { Id = Guid.NewGuid(), Name = "TestCorp" };

    private static OrbitParameters ValidOrbit() =>
        new OrbitParameters(
            semiMajorAxis:     6778,
            eccentricity:      0.001,
            inclination:       51.6,
            RAAN:              0,
            argumentOfPerigee: 0,
            meanAnomaly:       0);

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidArguments_ReturnsSatelliteWithActiveStatus()
    {
        var sat = Satellite.Create("ISS", DateTime.UtcNow.AddYears(-5), MakeOperator());

        sat.Status.Should().Be(ESatelliteStatus.Active);
    }

    [Fact]
    public void Create_WithValidArguments_SetsOperatorIdFromOperator()
    {
        var op  = MakeOperator();
        var sat = Satellite.Create("ISS", DateTime.UtcNow.AddYears(-5), op);

        sat.OperatorId.Should().Be(op.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceName_ThrowsArgumentException(string? name)
    {
        Action act = () => Satellite.Create(name!, DateTime.UtcNow, MakeOperator());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Always_AssignsNewId()
    {
        var op = MakeOperator();
        var s1 = Satellite.Create("A", DateTime.UtcNow, op);
        var s2 = Satellite.Create("B", DateTime.UtcNow, op);

        s1.Id.Should().NotBe(s2.Id);
    }

    // ── CreateFromExternalData ────────────────────────────────────────────────

    [Fact]
    public void CreateFromExternalData_WithValidName_ReturnsSatelliteWithActiveStatus()
    {
        var sat = Satellite.CreateFromExternalData("Starlink-1", DateTime.UtcNow.AddYears(-1), "SpaceX", null);

        sat.Status.Should().Be(ESatelliteStatus.Active);
    }

    [Fact]
    public void CreateFromExternalData_WithNullOperatorName_SetsOperatorNameToEmpty()
    {
        var sat = Satellite.CreateFromExternalData("TestSat", DateTime.UtcNow, null, null);

        sat.OperatorName.Should().BeEmpty();
    }

    [Fact]
    public void CreateFromExternalData_WithOrbitParameters_SetsOrbit()
    {
        var sat = Satellite.CreateFromExternalData("TestSat", DateTime.UtcNow, null, ValidOrbit());

        sat.Orbit.Should().NotBeNull();
    }

    [Fact]
    public void CreateFromExternalData_WithNullOrbitParameters_LeavesOrbitNull()
    {
        var sat = Satellite.CreateFromExternalData("TestSat", DateTime.UtcNow, null, null);

        sat.Orbit.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateFromExternalData_WithEmptyName_ThrowsArgumentException(string? name)
    {
        Action act = () => Satellite.CreateFromExternalData(name!, DateTime.UtcNow, null, null);

        act.Should().Throw<ArgumentException>();
    }

    // ── SoftDelete ────────────────────────────────────────────────────────────

    [Fact]
    public void SoftDelete_WhenCalled_SetsStatusToDeleted()
    {
        var sat = Satellite.Create("ISS", DateTime.UtcNow.AddYears(-5), MakeOperator());

        sat.SoftDelete("admin");

        sat.Status.Should().Be(ESatelliteStatus.Deleted);
    }

    [Fact]
    public void SoftDelete_WhenCalled_SetsDeletedAt()
    {
        var sat          = Satellite.Create("ISS", DateTime.UtcNow.AddYears(-5), MakeOperator());
        var beforeDelete = DateTime.UtcNow;

        sat.SoftDelete("admin");

        sat.DeletedAt.Should().BeOnOrAfter(beforeDelete);
    }

    [Fact]
    public void SoftDelete_WhenCalled_SetsDeletedByToProvidedUser()
    {
        var sat = Satellite.Create("ISS", DateTime.UtcNow.AddYears(-5), MakeOperator());

        sat.SoftDelete("admin");

        sat.DeletedBy.Should().Be("admin");
    }
}