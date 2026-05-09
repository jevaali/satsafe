using App.Domain.Entities;
using App.Domain.ValueObjects;
using AwesomeAssertions;

namespace App.Tests.Unit.Domain.Entities;

public class DraftSatelliteTests
{
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

    [Fact]
    public void Create_WithValidArguments_ReturnsDraftSatelliteWithTrimmedName()
    {
        var userId = Guid.NewGuid();

        var draft = DraftSatellite.Create(userId, "  MySat  ", null, ValidOrbit());

        draft.Name.Should().Be("MySat");
    }

    [Fact]
    public void Create_WithValidArguments_SetsUserIdCorrectly()
    {
        var userId = Guid.NewGuid();

        var draft = DraftSatellite.Create(userId, "MySat", null, ValidOrbit());

        draft.UserId.Should().Be(userId);
    }

    [Fact]
    public void Create_WithNotes_StoresTrimmedNotes()
    {
        var draft = DraftSatellite.Create(Guid.NewGuid(), "MySat", "  some notes  ", ValidOrbit());

        draft.Notes.Should().Be("some notes");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceName_ThrowsArgumentException(string? name)
    {
        Action act = () => DraftSatellite.Create(Guid.NewGuid(), name!, null, ValidOrbit());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Always_AssignsNewId()
    {
        var d1 = DraftSatellite.Create(Guid.NewGuid(), "A", null, ValidOrbit());
        var d2 = DraftSatellite.Create(Guid.NewGuid(), "B", null, ValidOrbit());

        d1.Id.Should().NotBe(d2.Id);
    }

    [Fact]
    public void Update_WithValidArguments_UpdatesName()
    {
        var draft = DraftSatellite.Create(Guid.NewGuid(), "Original", null, ValidOrbit());

        draft.Update("Updated", null, ValidOrbit());

        draft.Name.Should().Be("Updated");
    }

    [Fact]
    public void Update_WithValidArguments_UpdatesParameters()
    {
        var draft = DraftSatellite.Create(Guid.NewGuid(), "MySat", null, ValidOrbit());

        draft.Update("MySat", null, OrbitWithInclination(90));

        draft.Parameters.Inclination.Should().Be(90);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ThrowsArgumentException(string name)
    {
        var draft = DraftSatellite.Create(Guid.NewGuid(), "MySat", null, ValidOrbit());

        Action act = () => draft.Update(name, null, ValidOrbit());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_WithValidArguments_SetsUpdatedAt()
    {
        var draft        = DraftSatellite.Create(Guid.NewGuid(), "MySat", null, ValidOrbit());
        var beforeUpdate = DateTime.UtcNow;

        draft.Update("NewName", null, ValidOrbit());

        draft.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }
}