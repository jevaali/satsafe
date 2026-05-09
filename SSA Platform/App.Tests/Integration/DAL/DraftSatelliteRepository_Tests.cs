using App.Tests.Integration.Helpers;

namespace App.Tests.Integration.DAL;

using Application.Mappers;
using Domain.Entities;
using Domain.ValueObjects;
using Infrastructure.Repositories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

public class DraftSatelliteRepository_Tests : RepositoryTestBase
{
    private static OrbitParameters SampleOrbit() =>
        new(7000e3, 0.001, 51.6, 0, 0, 0);

    private DraftSatelliteRepository CreateRepo() =>
        new(CreateContext(), new IdentityMapper<DraftSatellite>());

    private async Task<Guid> AddDraftAsync(Guid userId, string name)
    {
        await using var ctx = CreateContext();
        var draft = DraftSatellite.Create(userId, name, null, SampleOrbit());
        ctx.DraftSatellites.Add(draft);
        await ctx.SaveChangesAsync();
        return draft.Id;
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyUsersDrafts()
    {
        await AddDraftAsync(TestUsers.UserAId, "sat-a");
        await AddDraftAsync(TestUsers.UserBId, "sat-b");

        await using var ctx = CreateContext();
        var repo = new DraftSatelliteRepository(ctx, new IdentityMapper<DraftSatellite>());

        var results = (await repo.GetByUserIdAsync(TestUsers.UserAId)).ToList();

        results.Should().HaveCount(1);
        results[0].Name.Should().Be("sat-a");
    }

    [Fact]
    public async Task GetByIdAndUserAsync_WithMatchingUser_ReturnsDraft()
    {
        var id = await AddDraftAsync(TestUsers.UserAId, "my-sat");

        await using var ctx = CreateContext();
        var repo = new DraftSatelliteRepository(ctx, new IdentityMapper<DraftSatellite>());

        var result = await repo.GetByIdAndUserAsync(id, TestUsers.UserAId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("my-sat");
    }

    [Fact]
    public async Task GetByIdAndUserAsync_WithMismatchedUser_ReturnsNull_IDOR()
    {
        var id = await AddDraftAsync(TestUsers.UserAId, "my-sat");

        await using var ctx = CreateContext();
        var repo = new DraftSatelliteRepository(ctx, new IdentityMapper<DraftSatellite>());

        var result = await repo.GetByIdAndUserAsync(id, TestUsers.UserBId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_PersistsDraftWithCorrectOwner()
    {
        var draft = DraftSatellite.Create(TestUsers.UserAId, "new-sat", "some notes", SampleOrbit());

        await using (var ctx = CreateContext())
        {
            var repo = new DraftSatelliteRepository(ctx, new IdentityMapper<DraftSatellite>());
            repo.Add(draft);
            await ctx.SaveChangesAsync();
        }

        await using var verify = CreateContext();
        var stored = await verify.DraftSatellites.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == draft.Id);

        stored.Should().NotBeNull();
        stored!.UserId.Should().Be(TestUsers.UserAId);
        stored.Name.Should().Be("new-sat");
        stored.Notes.Should().Be("some notes");
    }

    [Fact]
    public async Task FindAsync_ById_ReturnsDraft()
    {
        var id = await AddDraftAsync(TestUsers.UserAId, "find-me");

        await using var ctx = CreateContext();
        var repo = new DraftSatelliteRepository(ctx, new IdentityMapper<DraftSatellite>());

        var result = await repo.FindAsync(id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("find-me");
    }

    [Fact]
    public async Task RemoveAsync_ById_RemovesDraft()
    {
        var id = await AddDraftAsync(TestUsers.UserAId, "to-delete");

        await using (var ctx = CreateContext())
        {
            var repo = new DraftSatelliteRepository(ctx, new IdentityMapper<DraftSatellite>());
            await repo.RemoveAsync(id);
            await ctx.SaveChangesAsync();
        }

        await using var verify = CreateContext();
        (await verify.DraftSatellites.AsNoTracking().AnyAsync(d => d.Id == id)).Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_OrdersByUpdatedAtThenCreatedAt_Descending()
    {
        await using var ctx = CreateContext();

        var older = DraftSatellite.Create(TestUsers.UserAId, "older", null, SampleOrbit());
        older.CreatedAt = DateTime.UtcNow.AddDays(-2);

        var newer = DraftSatellite.Create(TestUsers.UserAId, "newer", null, SampleOrbit());
        newer.CreatedAt = DateTime.UtcNow.AddDays(-1);

        ctx.DraftSatellites.AddRange(older, newer);
        await ctx.SaveChangesAsync();

        await using var readCtx = CreateContext();
        var repo = new DraftSatelliteRepository(readCtx, new IdentityMapper<DraftSatellite>());
        var results = (await repo.GetByUserIdAsync(TestUsers.UserAId)).ToList();

        results.Should().HaveCount(2);
        results[0].Name.Should().Be("newer");
        results[1].Name.Should().Be("older");
    }
}
