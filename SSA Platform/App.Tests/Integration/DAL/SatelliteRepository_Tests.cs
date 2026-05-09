namespace App.Tests.Integration.DAL;

using App.Application.Mappers;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Infrastructure.Repositories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

public class SatelliteRepository_Tests : RepositoryTestBase
{
    private async Task<Operator> AddOperatorAsync()
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
        return op;
    }

    private async Task<Satellite> AddSatelliteAsync(Operator op, string name, ESatelliteStatus status = ESatelliteStatus.Active)
    {
        await using var ctx = CreateContext();
        ctx.Operators.Attach(op);
        var sat = Satellite.Create(name, DateTime.UtcNow, op);
        if (status != ESatelliteStatus.Active)
            sat.SoftDelete("test");
        ctx.Satellites.Add(sat);
        await ctx.SaveChangesAsync();
        return sat;
    }

    [Fact]
    public async Task GetByOperatorIdAsync_ReturnsOnlyThatOperatorsSatellites()
    {
        var opA = await AddOperatorAsync();
        var opB = await AddOperatorAsync();

        await AddSatelliteAsync(opA, "A-sat");
        await AddSatelliteAsync(opB, "B-sat");

        await using var ctx = CreateContext();
        var repo = new SatelliteRepository(ctx, new IdentityMapper<Satellite>());

        var results = (await repo.GetByOperatorIdAsync(opA.Id)).ToList();

        results.Should().HaveCount(1);
        results[0].Name.ToString().Should().Be("A-sat");
    }

    [Fact]
    public async Task GetByOperatorIdAsync_ExcludesDeletedSatellites()
    {
        var op = await AddOperatorAsync();
        await AddSatelliteAsync(op, "active-sat");
        await AddSatelliteAsync(op, "deleted-sat", ESatelliteStatus.Deleted);

        await using var ctx = CreateContext();
        var repo = new SatelliteRepository(ctx, new IdentityMapper<Satellite>());

        var results = (await repo.GetByOperatorIdAsync(op.Id)).ToList();

        results.Should().HaveCount(1);
        results[0].Name.ToString().Should().Be("active-sat");
    }

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        var op = await AddOperatorAsync();
        await AddSatelliteAsync(op, "sat-1");
        await AddSatelliteAsync(op, "sat-2");

        await using var ctx = CreateContext();
        var repo = new SatelliteRepository(ctx, new IdentityMapper<Satellite>());

        var count = await repo.GetCountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task AllAsync_ReturnsAllSatellites()
    {
        var opA = await AddOperatorAsync();
        var opB = await AddOperatorAsync();
        await AddSatelliteAsync(opA, "sat-a");
        await AddSatelliteAsync(opB, "sat-b");

        await using var ctx = CreateContext();
        var repo = new SatelliteRepository(ctx, new IdentityMapper<Satellite>());

        var all = (await repo.AllAsync()).ToList();

        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_ById_ReturnsSatellite()
    {
        var op = await AddOperatorAsync();
        var sat = await AddSatelliteAsync(op, "find-me");

        await using var ctx = CreateContext();
        var repo = new SatelliteRepository(ctx, new IdentityMapper<Satellite>());

        var result = await repo.FindAsync(sat.Id);

        result.Should().NotBeNull();
        result!.Name.ToString().Should().Be("find-me");
    }

    [Fact]
    public async Task GetActiveCountAsync_ExcludesNonActive()
    {
        var op = await AddOperatorAsync();
        await AddSatelliteAsync(op, "active");
        await AddSatelliteAsync(op, "deleted", ESatelliteStatus.Deleted);

        await using var ctx = CreateContext();
        var repo = new SatelliteRepository(ctx, new IdentityMapper<Satellite>());

        var count = await repo.GetActiveCountAsync();

        count.Should().Be(1);
    }
}
