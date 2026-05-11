using App.Tests.Helpers;
using AwesomeAssertions;
using Microsoft.Playwright;
using WebApp.Tests.E2E;
using Xunit.Abstractions;

namespace App.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class SatelliteLanguageE2E
{
    private readonly PlaywrightWebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    public SatelliteLanguageE2E(PlaywrightWebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    private static string FreshEmail(string label) =>
        $"{label}+{Guid.NewGuid():N}@test.ee";

    private static readonly PageGotoOptions DomReady = new() { WaitUntil = WaitUntilState.DOMContentLoaded };

    private async Task<(IPlaywright pw, IBrowser browser, IPage page)> NewBrowserAsync()
    {
        var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.ConnectAsync("ws://localhost:3000/");
        var page = await (await browser.NewContextAsync()).NewPageAsync();
        return (pw, browser, page);
    }

    [Fact]
    public async Task SatelliteName_RoundTrip_PreservesBothTranslations()
    {
        var (pw, browser, page) = await NewBrowserAsync();
        try
        {
            await PlaywrightLoginHelper.RegisterOperatorAsync(page, _factory.RootUri,
                FreshEmail("lang"), "Test.pass1");

            _output.WriteLine($"[AFTER-REGISTER] URL={page.Url}");

            // 1. Switch to en
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "en");

            // 2. Create satellite with EN name
            var satBase = $"sat-{Guid.NewGuid():N}";
            var satNameEn = $"{satBase}-en";

            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite/Create", DomReady);
            await page.FillAsync("#Name", satNameEn);
            await page.FillAsync("#LaunchDate", "2020-01-15");
            await page.FillAsync("#NoradId", "99001");
            await page.FillAsync("#SemiMajorAxis", "6778000");
            await page.FillAsync("#Eccentricity", "0.001");
            await page.FillAsync("#Inclination", "98.2");
            await page.FillAsync("#RAAN", "235.6");
            await page.FillAsync("#ArgumentOfPerigee", "90.0");
            await page.FillAsync("#MeanAnomaly", "0.0");

            await page.RunAndWaitForNavigationAsync(
                async () => await page.ClickAsync("button.btn-add"),
                new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30_000 });

            _output.WriteLine($"[AFTER-CREATE] URL={page.Url}");

            // 3. Switch to et
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "et");

            // 4. Edit the satellite; set ET name
            var satNameEt = $"{satBase}-et";
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);

            var card = page.Locator(".sat-card").Filter(new() { HasTextString = satNameEn });
            await card.Locator("a.btn-sat-edit, a:has-text('Edit')").First.ClickAsync();
            await page.WaitForURLAsync(
                new System.Text.RegularExpressions.Regex(".*/Operator/Satellite/Edit/.*"),
                new PageWaitForURLOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 15_000 });

            _output.WriteLine($"[EDIT] URL={page.Url}");

            await page.FillAsync("#Name", satNameEt);
            await page.RunAndWaitForNavigationAsync(
                async () => await page.ClickAsync("form:not([action='/Login/Logout']) button[type=submit]"),
                new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30_000 });

            // 5. While in et: index shows ET name
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);
            var etText = await page.Locator("body").TextContentAsync();
            _output.WriteLine($"[ET-INDEX] Body contains satNameEt={etText!.Contains(satNameEt)}");
            etText.Should().Contain(satNameEt);

            // 6. Switch back to en
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "en");
            await page.GotoAsync($"{_factory.RootUri}/Operator/Satellite", DomReady);

            // 7. While in en: index shows original EN name
            var enText = await page.Locator("body").TextContentAsync();
            _output.WriteLine($"[EN-INDEX] Body contains satNameEn={enText!.Contains(satNameEn)}");
            enText.Should().Contain(satNameEn);
            enText.Should().NotContain(satNameEt);
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }

    [Fact]
    public async Task LanguageSwitcher_PersistsCookie()
    {
        var (pw, browser, page) = await NewBrowserAsync();
        try
        {
            await PlaywrightLoginHelper.RegisterOperatorAsync(page, _factory.RootUri,
                FreshEmail("lang-switch"), "Test.pass1");

            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "et");

            var cookies = await page.Context.CookiesAsync();
            cookies.Should().Contain(c => c.Name == ".AspNetCore.Culture");
            var cultureCookie = cookies.First(c => c.Name == ".AspNetCore.Culture");
            _output.WriteLine($"[COOKIE] Culture cookie value: {cultureCookie.Value}");
            cultureCookie.Value.Should().Contain("et");
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }
}