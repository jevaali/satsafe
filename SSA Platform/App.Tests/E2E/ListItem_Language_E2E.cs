using App.Tests.Integration.Helpers;
using AwesomeAssertions;
using Microsoft.Playwright;
using WebApp.Tests.E2E;

namespace App.Tests.E2E;

[Trait("Category", "E2E")]
[Collection("E2E")]
public class ListItem_Language_E2E
{
    private readonly PlaywrightWebAppFactory _factory;

    public ListItem_Language_E2E(PlaywrightWebAppFactory factory)
    {
        _factory = factory;
    }

    private static string FreshEmail(string label) =>
        $"{label}+{Guid.NewGuid():N}@test.ee";

    private async Task<(IPlaywright pw, IBrowser browser, IPage page)> NewBrowserAsync()
    {
        var pw = await Playwright.CreateAsync();
        var browser = await pw.Chromium.ConnectAsync("ws://localhost:3000/");
        var page = await (await browser.NewContextAsync()).NewPageAsync();
        return (pw, browser, page);
    }

    [Fact]
    public async Task LangStr_RoundTrip_PreservesBothTranslations()
    {
        var (pw, browser, page) = await NewBrowserAsync();
        try
        {
            await PlaywrightLoginHelper.RegisterAsync(page, _factory.RootUri,
                FreshEmail("lang"), TestUsers.Password);

            // 1. Switch to en
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "en");

            // 2. Create item with EN summary
            var description = $"lang-{Guid.NewGuid():N}";
            await page.GotoAsync($"{_factory.RootUri}/ListItems/Create");
            await page.FillAsync("#ItemDescription", description);
            await page.FillAsync("#Summary", "hello");
            await page.ClickAsync("input[type=submit]");
            await page.WaitForURLAsync($"{_factory.RootUri}/ListItems");

            // 3. Switch to et
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "et");

            // 4. Edit the same item; set ET summary
            await page.GotoAsync($"{_factory.RootUri}/ListItems");
            var row = page.Locator("tr").Filter(new() { HasTextString = description });
            await row.Locator("a:has-text('Edit')").First.ClickAsync();
            await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/ListItems/Edit/.*"));
            await page.FillAsync("#Summary", "tere");
            await page.ClickAsync("input[type=submit]");
            await page.WaitForURLAsync($"{_factory.RootUri}/ListItems");

            // 5. While in et: Index shows "tere"
            (await page.Locator("body").TextContentAsync()).Should().Contain("tere");

            // 6. Switch back to en
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "en");
            await page.GotoAsync($"{_factory.RootUri}/ListItems");

            // 7. While in en: Index shows the ORIGINAL "hello" (en translation preserved)
            var inEnText = await page.Locator("body").TextContentAsync();
            inEnText.Should().Contain("hello");
            // Sanity: the et value must not have overwritten the en value
            inEnText.Should().NotContain("tere");
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
            await PlaywrightLoginHelper.RegisterAsync(page, _factory.RootUri,
                FreshEmail("lang-switch"), TestUsers.Password);

            // Switch to et and verify the cookie is set
            await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "et");

            var ctx = page.Context;
            var cookies = await ctx.CookiesAsync();
            cookies.Should().Contain(c => c.Name == ".AspNetCore.Culture");
            var cultureCookie = cookies.First(c => c.Name == ".AspNetCore.Culture");
            cultureCookie.Value.Should().Contain("et");
        }
        finally
        {
            await browser.DisposeAsync();
            pw.Dispose();
        }
    }
}
