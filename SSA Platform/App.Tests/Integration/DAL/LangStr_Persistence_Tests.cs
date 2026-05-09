namespace App.Tests.Integration.DAL;

using Domain.Entities;
using AwesomeAssertions;
using Base.Domain;
using Microsoft.EntityFrameworkCore;

[Collection("CultureSensitive")]
public class LangStr_Persistence_Tests : RepositoryTestBase
{
    public LangStr_Persistence_Tests()
    {
        LangStr.DefaultCulture = "en";
    }

    private async Task<Guid> AddSatelliteWithNameAsync(LangStr name)
    {
        await using var ctx = CreateContext();

        var op = new Operator
        {
            Id = Guid.NewGuid(),
            Name = "Test Op",
            CountryCode = "EE",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
        ctx.Operators.Add(op);
        await ctx.SaveChangesAsync();

        var sat = Satellite.CreateFromExternalData(name["en"], DateTime.UtcNow, null, null);
        foreach (var (culture, value) in name)
        {
            sat.Name[culture] = value;
        }
        ctx.Satellites.Add(sat);
        await ctx.SaveChangesAsync();

        return sat.Id;
    }

    [Fact]
    public async Task LangStr_RoundTripsThroughDb_WithMultipleCultures()
    {
        var id = await AddSatelliteWithNameAsync(
            new LangStr { ["en"] = "hello", ["et"] = "tere", ["de"] = "hallo" });

        await using var verify = CreateContext();
        var stored = await verify.Satellites.AsNoTracking().FirstAsync(s => s.Id == id);

        stored.Name.Should().BeOfType<LangStr>();
        stored.Name.Should().HaveCount(3);
        stored.Name["en"].Should().Be("hello");
        stored.Name["et"].Should().Be("tere");
        stored.Name["de"].Should().Be("hallo");
    }

    [Fact]
    public async Task LangStr_PersistedAsJsonText_QueryableAsRawString()
    {
        var id = await AddSatelliteWithNameAsync(
            new LangStr { ["en"] = "hello", ["et"] = "tere" });

        await using var cmd = Connection.CreateCommand();
        cmd.CommandText = "SELECT Name FROM SpaceObjects WHERE SpaceObjectType = 'Satellite' LIMIT 1";
        var raw = (string)(await cmd.ExecuteScalarAsync())!;

        raw.Should().Contain("\"en\"").And.Contain("\"et\"");
        raw.Should().Contain("hello").And.Contain("tere");
    }

    [Fact]
    public async Task LangStr_AddingNewCulture_PreservesExistingTranslations()
    {
        var id = await AddSatelliteWithNameAsync(
            new LangStr { ["en"] = "hello" });

        await using (var ctx = CreateContext())
        {
            var sat = await ctx.Satellites.FirstAsync(s => s.Id == id);
            sat.Name["et"] = "tere";
            ctx.Entry(sat).Property(s => s.Name).IsModified = true;
            await ctx.SaveChangesAsync();
        }

        await using var verify = CreateContext();
        var reloaded = await verify.Satellites.AsNoTracking().FirstAsync(s => s.Id == id);
        reloaded.Name["en"].Should().Be("hello");
        reloaded.Name["et"].Should().Be("tere");
    }
}
