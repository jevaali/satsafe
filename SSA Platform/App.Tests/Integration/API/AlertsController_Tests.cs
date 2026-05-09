namespace App.Tests.Integration.API;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Features.Alerts.DTOs;
using Infrastructure.Persistence;
using Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[Collection("CultureSensitive")]
public class AlertsController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AlertsController_Tests(CustomWebApplicationFactory factory)
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
        var res = await CreateClient().GetAsync("/api/v1/operator/alerts/list");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── List ─────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_WithJwt_Returns200_ContainsSeededAlert()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var alerts = await client.GetFromJsonAsync<List<AlertSummaryDto>>(
            "/api/v1/operator/alerts/list");

        alerts.Should().NotBeNull();
        alerts.Should().Contain(a => a.Id == TestUsers.TestAlertId);
    }

    [Fact]
    public async Task List_AsHelper_Returns200()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserBEmail);
        var res = await client.GetAsync("/api/v1/operator/alerts/list");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Details ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Details_SeededAlert_Returns200()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.GetAsync(
            $"/api/v1/operator/alerts/details/{TestUsers.TestAlertId}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await res.Content.ReadFromJsonAsync<AlertDetailsDto>();
        dto.Should().NotBeNull();
        dto.Id.Should().Be(TestUsers.TestAlertId);
    }

    [Fact]
    public async Task Details_UnknownId_Returns404()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.GetAsync(
            $"/api/v1/operator/alerts/details/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Acknowledge ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Acknowledge_SeededAlert_Returns204_AndSetsAcknowledged()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PostAsync(
            $"/api/v1/operator/alerts/acknowledge/{TestUsers.TestAlertId}", null);

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await ctx.Alerts.AsNoTracking()
            .FirstAsync(a => a.Id == TestUsers.TestAlertId);
        stored.IsAcknowledged.Should().BeTrue();
    }

    [Fact]
    public async Task Acknowledge_UnknownAlert_Returns404()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PostAsync(
            $"/api/v1/operator/alerts/acknowledge/{Guid.NewGuid()}", null);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── UpdateMessage ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateMessage_ValidMessage_Returns204_AndPersists()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PutAsJsonAsync(
            $"/api/v1/operator/alerts/updatemessage/{TestUsers.TestAlertId}",
            "updated message text");

        res.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await ctx.Alerts.AsNoTracking()
            .FirstAsync(a => a.Id == TestUsers.TestAlertId);
        stored.Message?.ToString().Should().Be("updated message text");
    }

    [Fact]
    public async Task UpdateMessage_EmptyMessage_Returns400()
    {
        var client = await AuthenticatedClientAsync(TestUsers.UserAEmail);
        var res = await client.PutAsJsonAsync(
            $"/api/v1/operator/alerts/updatemessage/{TestUsers.TestAlertId}", "");
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
