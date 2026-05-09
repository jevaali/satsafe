using App.Tests.Integration.Helpers;

namespace App.Tests.Integration.API;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTO.v1.Identity;
using AwesomeAssertions;

[Collection("CultureSensitive")]
public class AccountController_Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AccountController_Tests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    // ── Login ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_Returns200_WithJwt()
    {
        var res = await CreateClient().PostAsJsonAsync(
            "/api/v1/identity/account/login",
            new LoginInfo { Email = TestUsers.UserAEmail, Password = TestUsers.Password });

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadFromJsonAsync<JWTResponse>();
        body.Should().NotBeNull();
        body.Jwt.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns404()
    {
        var res = await CreateClient().PostAsJsonAsync(
            "/api/v1/identity/account/login",
            new LoginInfo { Email = TestUsers.UserAEmail, Password = "wrong-password!" });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns404()
    {
        var res = await CreateClient().PostAsJsonAsync(
            "/api/v1/identity/account/login",
            new LoginInfo { Email = "nobody@test.ee", Password = TestUsers.Password });

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Register ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_NewUser_Returns200_WithJwt()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];
        var res = await CreateClient().PostAsJsonAsync(
            "/api/v1/identity/account/register",
            new RegisterInfo
            {
                Email = $"new-{unique}@test.ee",
                Password = "NewUser.Pass1",
                Firstname = "Test",
                Lastname = "User",
            });

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadFromJsonAsync<JWTResponse>();
        body.Should().NotBeNull();
        body.Jwt.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var res = await CreateClient().PostAsJsonAsync(
            "/api/v1/identity/account/register",
            new RegisterInfo
            {
                Email = TestUsers.UserAEmail, // already seeded
                Password = TestUsers.Password,
                Firstname = "Test",
                Lastname = "User",
            });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Logout ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidJwt_Returns200()
    {
        var client = CreateClient();

        // Login first to get a real refresh token
        var loginRes = await client.PostAsJsonAsync(
            "/api/v1/identity/account/login",
            new LoginInfo { Email = TestUsers.UserAEmail, Password = TestUsers.Password });
        var body = await loginRes.Content.ReadFromJsonAsync<JWTResponse>();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Jwt);

        var res = await client.PostAsJsonAsync(
            "/api/v1/identity/account/logout",
            new LogoutInfo { RefreshToken = body.RefreshToken });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_NoJwt_Returns401()
    {
        var res = await CreateClient().PostAsJsonAsync(
            "/api/v1/identity/account/logout",
            new LogoutInfo { RefreshToken = "dummy" });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
