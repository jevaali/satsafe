using AwesomeAssertions;
using Microsoft.Playwright;
using WebApp.Tests.E2E;
using Xunit.Abstractions;

namespace App.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class SatelliteCrud_E2E
{
    private readonly PlaywrightWebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    public SatelliteCrud_E2E(PlaywrightWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
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
        _output.WriteLine($"[REGISTER] Page loaded: {page.Url}");

        await page.FillAsync("#FirstName", "Test");
        await page.FillAsync("#LastName", "Operator");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);
        await page.FillAsync("#ConfirmPassword", password);
        await page.FillAsync("#OperatorName", "TestOrg-" + Guid.NewGuid().ToString("N")[..8]);
        await page.SelectOptionAsync("#country-select", new SelectOptionValue { Value = "US" });
        await page.SelectOptionAsync("#OperatorType", new SelectOptionValue { Label = "Commercial" });

        _output.WriteLine($"[REGISTER] OperatorCountry={await page.InputValueAsync("#country-select")}");
        _output.WriteLine($"[REGISTER] OperatorType={await page.InputValueAsync("#OperatorType")}");

        var btn = page.Locator("button[type=submit]");
        await page.RunAndWaitForNavigationAsync(
            async () => await btn.ClickAsync(),
            new PageRunAndWaitForNavigationOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

        _output.WriteLine($"[AFTER-REGISTER] URL={page.Url}");

        if (page.Url.Contains("/Register/Operator", StringComparison.OrdinalIgnoreCase))
        {
            var h1 = await page.Locator("h1").First.InnerTextAsync();
            _output.WriteLine($"[REGISTER-FAIL] H1={h1}");

            var titleError = page.Locator(".titleerror");
            if (await titleError.CountAsync() > 0)
                _output.WriteLine($"[REGISTER-FAIL] Detail={await titleError.First.InnerTextAsync()}");

            var allDanger = await page.Locator(".text-danger").AllInnerTextsAsync();
            foreach (var e in allDanger.Where(e => !string.IsNullOrWhiteSpace(e)))
                _output.WriteLine($"[REGISTER-FAIL] ValidationError={e}");

            throw new InvalidOperationException($"Operator registration failed for {email}.");
        }
    }

    [Fact]
    public async Task Satellite_Index_Anonymous_RedirectsToLogin()
    {
        var (pw, browser) = await NewBrowserAsync();
        try
        {
            var page = await NewPageAsync(browser);
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            _output.WriteLine($"[ANON-REDIRECT] URL={page.Url}");
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
                _output.WriteLine($"[RESPONSE] {r.Request.Method} {r.Status} {r.Url}");

            var rootResponse = await page.GotoAsync($"{_factory.RootUri}",
                new PageGotoOptions { Timeout = 10_000, WaitUntil = WaitUntilState.DOMContentLoaded });
            _output.WriteLine($"[ROOT] Status={rootResponse?.Status} URL={page.Url}");

            // Register — already signs in automatically
            await RegisterOperatorAsync(page, email, password);
            _output.WriteLine($"[AFTER-REGISTER] URL={page.Url}");

            // Navigate to satellite index — should be empty
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            _output.WriteLine($"[SAT-INDEX] URL={page.Url}");
            (await page.Locator("body").TextContentAsync()).Should().NotContain(satName);

            // Create a satellite
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite/Create", DomReady);
            _output.WriteLine($"[SAT-CREATE] URL={page.Url}");

            await page.FillAsync("#Name", satName);
            await page.FillAsync("#LaunchDate", "2020-01-15");
            await page.FillAsync("#NoradId", "99001");
            await page.FillAsync("#SemiMajorAxis", "6778000");
            await page.FillAsync("#Eccentricity", "0.001");
            await page.FillAsync("#Inclination", "98.2");
            await page.FillAsync("#RAAN", "235.6");
            await page.FillAsync("#ArgumentOfPerigee", "90.0");
            await page.FillAsync("#MeanAnomaly", "0.0");

            var createBtn = page.Locator("button.btn-add");
            _output.WriteLine($"[BTN-CREATE] Visible={await createBtn.IsVisibleAsync()} Enabled={await createBtn.IsEnabledAsync()} Count={await createBtn.CountAsync()}");

            await page.RunAndWaitForNavigationAsync(
                async () => await createBtn.ClickAsync(),
                new PageRunAndWaitForNavigationOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30_000
                });

            _output.WriteLine($"[AFTER-CREATE] URL={page.Url}");

            if (page.Url.Contains("/Operator/Satellite/Create", StringComparison.OrdinalIgnoreCase))
            {
                var h1 = await page.Locator("h1").First.InnerTextAsync();
                _output.WriteLine($"[CREATE-FAIL] H1={h1}");

                var allDanger = await page.Locator(".text-danger").AllInnerTextsAsync();
                foreach (var e in allDanger.Where(e => !string.IsNullOrWhiteSpace(e)))
                    _output.WriteLine($"[CREATE-FAIL] ValidationError={e}");

                var body2 = await page.Locator("body").TextContentAsync() ?? string.Empty;
                throw new InvalidOperationException(
                    $"Satellite create failed. URL={page.Url}. H1={h1}. Body={body2[..Math.Min(500, body2.Length)]}");
            }

            // Navigate to satellite index — satellite should appear
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            _output.WriteLine($"[SAT-INDEX-AFTER-CREATE] URL={page.Url}");

            var body = await page.Locator("body").TextContentAsync();
            body.Should().Contain(satName);

            // Click delete on the satellite card
            var card = page.Locator(".sat-card").Filter(new() { HasTextString = satName });
            await card.Locator("a.btn-sat-delete").ClickAsync();
            await page.WaitForURLAsync(
                new System.Text.RegularExpressions.Regex("/Operator/Satellite/Delete/",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase),
                new PageWaitForURLOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15_000 });

            _output.WriteLine($"[SAT-DELETE] URL={page.Url}");

            var deleteBtn = page.Locator("form:not([action='/Login/Logout']) button[type=submit]");
            _output.WriteLine($"[BTN-DELETE] Visible={await deleteBtn.IsVisibleAsync()} Enabled={await deleteBtn.IsEnabledAsync()} Count={await deleteBtn.CountAsync()}");

            await page.RunAndWaitForNavigationAsync(
                async () => await deleteBtn.ClickAsync(),
                new PageRunAndWaitForNavigationOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 15_000
                });

            _output.WriteLine($"[AFTER-DELETE] URL={page.Url}");

            // Navigate back to index — satellite should be gone
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            _output.WriteLine($"[SAT-INDEX-AFTER-DELETE] URL={page.Url}");
            (await page.Locator("body").TextContentAsync()).Should().NotContain(satName);
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }
}