using Microsoft.Playwright;
using WebApp.Tests.E2E;
using Xunit.Abstractions;

namespace App.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class AuthE2ETests
{
    private readonly PlaywrightWebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    public AuthE2ETests(PlaywrightWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]

    public async Task User_Can_Register_And_Login()
    {
        var email = $"e2e+{Guid.NewGuid():N}@test.ee";
        const string password = "Test.pass1";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.ConnectAsync("ws://localhost:3000/");
        var page = await browser.NewPageAsync();
        
        // --- REGISTER ---
        await page.GotoAsync($"{_factory.RootUri}/Register/User");
        await page.FillAsync("#FirstName", "Jane");
        await page.FillAsync("#LastName", "Doe");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);
        await page.FillAsync("#ConfirmPassword", password);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _output.WriteLine("After register URL: " + page.Url);
        Assert.Contains("/User", page.Url, StringComparison.OrdinalIgnoreCase);

        // --- LOGOUT ---
        await page.ClickAsync("form[action='/Login/Logout'] button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _output.WriteLine("After logout URL: " + page.Url);

        // --- LOGIN ---
        await page.GotoAsync($"{_factory.RootUri}/Login");
        await page.WaitForSelectorAsync("#Email");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        _output.WriteLine("After login URL: " + page.Url);

        // --- ASSERT ---
        Assert.Contains("/User", page.Url, StringComparison.OrdinalIgnoreCase);
        await Assertions.Expect(page.Locator("form[action='/Login/Logout'] button[type='submit']")).ToBeVisibleAsync();}
    }