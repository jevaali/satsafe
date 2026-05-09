using AwesomeAssertions;
using Microsoft.Playwright;
using WebApp.Tests.E2E;

namespace App.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class SatelliteCrud_E2E
{
    private readonly PlaywrightWebAppFactory _factory;

    public SatelliteCrud_E2E(PlaywrightWebAppFactory factory)
    {
        _factory = factory;
    }

    private static string FreshEmail() =>
        $"op+{Guid.NewGuid():N}@test.ee";

    private async Task<(IPlaywright pw, IBrowser browser)> NewBrowserAsync()
    {
        var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.ConnectAsync("ws://localhost:3000/");
        return (pw, browser);
    }

    private async Task<IPage> NewPageAsync(IBrowser browser) =>
        await (await browser.NewContextAsync()).NewPageAsync();

    private static readonly PageGotoOptions DomReady = new() { WaitUntil = WaitUntilState.DOMContentLoaded };

    private async Task RegisterOperatorAsync(IPage page, string email, string password)
    {
        await page.GotoAsync($"{_factory.RootUri}/Register/Operator", DomReady);
        await page.FillAsync("#FirstName", "Test");
        await page.FillAsync("#LastName", "Operator");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);
        await page.FillAsync("#ConfirmPassword", password);
        await page.FillAsync("#OperatorName", "TestOrg-" + Guid.NewGuid().ToString("N")[..8]);
        await page.SelectOptionAsync("#country-select", new SelectOptionValue { Value = "US" });
        await page.SelectOptionAsync("#OperatorType", new SelectOptionValue { Label = "Commercial" });

        await page.ScreenshotAsync(new() { Path = "before-register-submit.png" });

        var btn = page.Locator("button[type=submit]");
        Console.WriteLine($"[BTN-REGISTER] Visible={await btn.IsVisibleAsync()} Enabled={await btn.IsEnabledAsync()} Count={await btn.CountAsync()}");

        await page.RunAndWaitForNavigationAsync(
            async () => await btn.ClickAsync(),
            new PageRunAndWaitForNavigationOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

        Console.WriteLine($"[AFTER-REGISTER] URL={page.Url}");

        if (page.Url.Contains("/Register/Operator", StringComparison.OrdinalIgnoreCase))
        {
            var errors = await page.Locator(".alert-danger, .text-danger").AllTextContentsAsync();
            throw new InvalidOperationException(
                $"Operator registration failed for {email}. Errors: {string.Join(" | ", errors.Where(e => !string.IsNullOrWhiteSpace(e)))}");
        }
    }

    private async Task LoginAsync(IPage page, string email, string password)
    {
        await page.GotoAsync($"{_factory.RootUri}/Login", DomReady);
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);

        await page.ScreenshotAsync(new() { Path = "before-login-submit.png" });

        var btn = page.Locator("button[type=submit]");
        Console.WriteLine($"[BTN-LOGIN] Visible={await btn.IsVisibleAsync()} Enabled={await btn.IsEnabledAsync()} Count={await btn.CountAsync()}");

        await page.RunAndWaitForNavigationAsync(
            async () => await btn.ClickAsync(),
            new PageRunAndWaitForNavigationOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

        Console.WriteLine($"[AFTER-LOGIN] URL={page.Url}");

        if (page.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase))
        {
            var errors = await page.Locator(".alert-danger, .text-danger").AllTextContentsAsync();
            throw new InvalidOperationException(
                $"Login failed for {email}. Errors: {string.Join(" | ", errors.Where(e => !string.IsNullOrWhiteSpace(e)))}");
        }
    }

    [Fact]
    public async Task Satellite_Index_Anonymous_RedirectsToLogin()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var page = await NewPageAsync(browser);
            page.Response += (_, r) =>
                Console.WriteLine($"[RESPONSE] {r.Request.Method} {r.Status} {r.Url}");

            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            page.Url.Should().Contain("/Login");
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    [Fact]
    public async Task Satellite_Create_Edit_Delete_RoundTrip()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var email = FreshEmail();
            const string password = "Test.pass1";
            var satName = "E2E-SAT-" + Guid.NewGuid().ToString("N")[..8];

            var page = await NewPageAsync(browser);

            page.Response += (_, r) =>
                Console.WriteLine($"[RESPONSE] {r.Request.Method} {r.Status} {r.Url}");

            // Sanity check — confirm Kestrel is reachable
            var rootResponse = await page.GotoAsync($"{_factory.RootUri}",
                new PageGotoOptions { Timeout = 10_000, WaitUntil = WaitUntilState.DOMContentLoaded });
            Console.WriteLine($"[ROOT] Status={rootResponse?.Status} URL={page.Url}");

            // Register operator and log in
            await RegisterOperatorAsync(page, email, password);
            await LoginAsync(page, email, password);

            // Navigate to satellite index — should be empty
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            (await page.Locator("body").TextContentAsync()).Should().NotContain(satName);

            // Create a satellite
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite/Create", DomReady);
            await page.FillAsync("#Name", satName);
            await page.FillAsync("#LaunchDate", "2020-01-15");
            await page.FillAsync("#NoradId", "99001");
            await page.FillAsync("#SemiMajorAxis", "6778000");
            await page.FillAsync("#Eccentricity", "0.001");
            await page.FillAsync("#Inclination", "98.2");
            await page.FillAsync("#RAAN", "235.6");
            await page.FillAsync("#ArgumentOfPerigee", "90.0");
            await page.FillAsync("#MeanAnomaly", "0.0");

            await page.ScreenshotAsync(new() { Path = "before-create-submit.png" });

            var createBtn = page.Locator("button[type=submit]");
            Console.WriteLine($"[BTN-CREATE] Visible={await createBtn.IsVisibleAsync()} Enabled={await createBtn.IsEnabledAsync()} Count={await createBtn.CountAsync()}");

            await page.RunAndWaitForNavigationAsync(
                async () => await createBtn.ClickAsync(),
                new PageRunAndWaitForNavigationOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30_000
                });

            Console.WriteLine($"[AFTER-CREATE] URL={page.Url}");

            if (page.Url.Contains("/Operator/Satellite/Create", StringComparison.OrdinalIgnoreCase))
            {
                var body2 = await page.Locator("body").TextContentAsync() ?? string.Empty;
                throw new InvalidOperationException(
                    $"Satellite create failed (form re-rendered). URL={page.Url}. Body={body2[..Math.Min(500, body2.Length)]}");
            }

            // Follow the redirect to the satellite index
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);

            // Satellite should appear on index
            var body = await page.Locator("body").TextContentAsync();
            body.Should().Contain(satName);

            // Navigate to the delete confirmation page
            var card = page.Locator(".sat-card").Filter(new() { HasTextString = satName });
            await card.Locator("a.btn-sat-delete").ClickAsync();
            await page.WaitForURLAsync(
                new System.Text.RegularExpressions.Regex("/Operator/Satellite/Delete/",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase),
                new PageWaitForURLOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15_000 });

            await page.ScreenshotAsync(new() { Path = "before-delete-submit.png" });

            var deleteBtn = page.Locator("button[type=submit]");
            Console.WriteLine($"[BTN-DELETE] Visible={await deleteBtn.IsVisibleAsync()} Enabled={await deleteBtn.IsEnabledAsync()} Count={await deleteBtn.CountAsync()}");

            await page.RunAndWaitForNavigationAsync(
                async () => await deleteBtn.ClickAsync(),
                new PageRunAndWaitForNavigationOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 15_000
                });

            Console.WriteLine($"[AFTER-DELETE] URL={page.Url}");

            // Navigate back to index to verify deletion
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            (await page.Locator("body").TextContentAsync()).Should().NotContain(satName);
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }
}