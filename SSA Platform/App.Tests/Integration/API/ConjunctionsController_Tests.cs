namespace App.Tests.Integration.API;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Features.Conjunctions.DTOs;
using Domain.Entities.Enums;
using Infrastructure.Persistence;
using Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[Collection("CultureSensitive")]
public class ConjunctionsController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ConjunctionsController_Tests(CustomWebApplicationFactory factory)
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

    // ── Authentication ──────────────────────────────────────────────────────────

    [Fact]
    public async Task List_NoJwt_Returns401()
    {
        var res = await CreateClient().GetAsync("/api/v1/operator/conjunctions/list");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── List ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_WithJwt_Returns200_ContainsSeededConjunction()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var conjunctions = await client.GetFromJsonAsync<List<ConjunctionSummaryDto>>(
            "/api/v1/operator/conjunctions/list");

        conjunctions.Should().NotBeNull();
        conjunctions.Should().Contain(c => c.Id == TestUsers.TestConjunctionId);
    }

    [Fact]
    public async Task List_AsHelper_Returns200()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserBEmail);
        var res = await client.GetAsync("/api/v1/operator/conjunctions/list");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Details ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_SeededConjunction_Returns200_WithCorrectOperatorSide()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.GetAsync(
            $"/api/v1/operator/conjunctions/details/{TestUsers.TestConjunctionId}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await res.Content.ReadFromJsonAsync<ConjunctionDetailDto>();
        dto.Should().NotBeNull();
        dto.Id.Should().Be(TestUsers.TestConjunctionId);
        // TestSatelliteId is the primary object; operator owns the primary side
        dto.OperatorSide.Should().Be("Primary");
    }

    [Fact]
    public async Task Details_UnknownId_Returns404()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.GetAsync(
            $"/api/v1/operator/conjunctions/details/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Acknowledge ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Acknowledge_NoJwt_Returns401()
    {
        var res = await CreateClient().PostAsJsonAsync(
            $"/api/v1/operator/conjunctions/acknowledge/{TestUsers.TestConjunctionId}",
            EOperatorAcknowledgement.Noticed);
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Acknowledge_WithNoticedState_Returns204_AndPersists()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PostAsJsonAsync(
            $"/api/v1/operator/conjunctions/acknowledge/{TestUsers.TestConjunctionId}",
            EOperatorAcknowledgement.Noticed);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await ctx.ConjunctionEvents.AsNoTracking()
            .FirstAsync(c => c.Id == TestUsers.TestConjunctionId);
        stored.PrimaryAcknowledgement.Should().Be(EOperatorAcknowledgement.Noticed);
    }

    [Fact]
    public async Task Acknowledge_WithResolvedState_Returns204_AndAutoAcknowledgesAlerts()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PostAsJsonAsync(
            $"/api/v1/operator/conjunctions/acknowledge/{TestUsers.TestConjunctionId}",
            EOperatorAcknowledgement.Resolved);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var alerts = await ctx.Alerts.AsNoTracking()
            .Where(a => a.ConjunctionEventId == TestUsers.TestConjunctionId)
            .ToListAsync();
        alerts.Should().AllSatisfy(a => a.IsAcknowledged.Should().BeTrue());
    }

    [Fact]
    public async Task Acknowledge_UnknownConjunction_Returns404()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PostAsJsonAsync(
            $"/api/v1/operator/conjunctions/acknowledge/{Guid.NewGuid()}",
            EOperatorAcknowledgement.Noticed);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
