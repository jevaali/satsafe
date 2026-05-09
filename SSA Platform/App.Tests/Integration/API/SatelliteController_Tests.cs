namespace App.Tests.Integration.API;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Features.Satellites.DTOs;
using Infrastructure.Persistence;
using Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[Collection("CultureSensitive")]
public class SatelliteController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SatelliteController_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    private async Task<HttpClient> AuthenticatedClientAsync(string email)
    {
        var client = CreateClient();
        var token = await JwtTestHelper.GenerateTokenAsync(_factory.Services, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static AddSatelliteRequest NewSatelliteRequest(string name = "api-test-sat") =>
        new()
        {
            Name = name,
            LaunchDate = DateTime.UtcNow.AddYears(-1),
            OperatorId = TestUsers.OperatorId,
            NoradId = 99999,
            SemiMajorAxis = 7000000,
            Eccentricity = 0.001,
            Inclination = 51.6,
            RAAN = 0,
            ArgumentOfPerigee = 0,
            MeanAnomaly = 0,
        };

    // ── Authentication ──────────────────────────────────────────────────────────

    [Fact]
    public async Task List_NoJwt_Returns401()
    {
        var res = await CreateClient().GetAsync("/api/v1/operator/satellite/list");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_WithJwt_Returns200_ContainsSeededSatellite()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var satellites = await client.GetFromJsonAsync<List<SatelliteSummaryDto>>(
            "/api/v1/operator/satellite/list");

        satellites.Should().NotBeNull();
        satellites.Should().Contain(s => s.Id == TestUsers.TestSatelliteId);
    }

    // ── Details ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_SeededSatellite_Returns200()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.GetAsync($"/api/v1/operator/satellite/details/{TestUsers.TestSatelliteId}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<SatelliteDetailDto>();
        dto.Should().NotBeNull();
        dto.Id.Should().Be(TestUsers.TestSatelliteId);
        dto.Name.Should().Be(TestUsers.TestSatelliteName);
    }

    [Fact]
    public async Task Details_UnknownId_Returns404()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.GetAsync($"/api/v1/operator/satellite/details/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Create ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_AsOperatorRole_Returns201_AndPersists()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PostAsJsonAsync("/api/v1/operator/satellite/create",
            NewSatelliteRequest("created-by-api"));

        res.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = await res.Content.ReadFromJsonAsync<Guid>();
        id.Should().NotBe(Guid.Empty);

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await ctx.Satellites.AsNoTracking().AnyAsync(s => s.Id == id)).Should().BeTrue();
    }

    [Fact]
    public async Task Create_AsHelperRole_Returns403()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserBEmail);
        var res = await client.PostAsJsonAsync("/api/v1/operator/satellite/create",
            NewSatelliteRequest());

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_MissingRequiredField_Returns400()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var badRequest = new { Name = "", LaunchDate = DateTime.UtcNow };
        var res = await client.PostAsJsonAsync("/api/v1/operator/satellite/create", badRequest);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Update ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_OwnSatellite_Returns204_AndPersistsChange()
    {
        // Create a fresh satellite so we don't mutate the shared seeded one
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var createRes = await client.PostAsJsonAsync("/api/v1/operator/satellite/create",
            NewSatelliteRequest("before-update"));
        createRes.EnsureSuccessStatusCode();
        var id = await createRes.Content.ReadFromJsonAsync<Guid>();

        var updateReq = new UpdateSatelliteRequest
        {
            SatelliteId = id,
            Name = "after-update",
            LaunchDate = DateTime.UtcNow.AddYears(-2),
            OperatorId = TestUsers.OperatorId,
            NoradId = 88888,
            SemiMajorAxis = 7100000,
            Eccentricity = 0.002,
            Inclination = 45.0,
            RAAN = 10,
            ArgumentOfPerigee = 5,
            MeanAnomaly = 20,
        };

        var res = await client.PutAsJsonAsync($"/api/v1/operator/satellite/update/{id}", updateReq);
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await ctx.Satellites.AsNoTracking().FirstAsync(s => s.Id == id);
        stored.Name.ContainsValue("after-update").Should().BeTrue();
    }

    [Fact]
    public async Task Update_AsHelperRole_Returns403()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserBEmail);
        var req = new UpdateSatelliteRequest
        {
            SatelliteId = TestUsers.TestSatelliteId,
            Name = "evil",
            LaunchDate = DateTime.UtcNow,
            OperatorId = TestUsers.OperatorId,
            NoradId = 1,
            SemiMajorAxis = 7000000,
            Eccentricity = 0,
            Inclination = 0,
            RAAN = 0,
            ArgumentOfPerigee = 0,
            MeanAnomaly = 0,
        };

        var res = await client.PutAsJsonAsync(
            $"/api/v1/operator/satellite/update/{TestUsers.TestSatelliteId}", req);
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Delete ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_CreatedSatellite_Returns204()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var id = await CreateSatelliteAsync(client, "to-delete");

        var res = await client.DeleteAsync($"/api/v1/operator/satellite/delete/{id}");
        res.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_UnknownSatellite_Returns404()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.DeleteAsync($"/api/v1/operator/satellite/delete/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsHelperRole_Returns403()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserBEmail);
        var res = await client.DeleteAsync(
            $"/api/v1/operator/satellite/delete/{TestUsers.TestSatelliteId}");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<Guid> CreateSatelliteAsync(HttpClient client, string name)
    {
        var res = await client.PostAsJsonAsync("/api/v1/operator/satellite/create",
            NewSatelliteRequest(name));
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<Guid>();
    }
}
