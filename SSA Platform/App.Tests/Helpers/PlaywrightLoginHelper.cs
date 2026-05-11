using Microsoft.Playwright;

namespace App.Tests.Helpers;

public static class PlaywrightLoginHelper
{
    public static async Task LoginAsync(IPage page, string rootUri, string email, string password)
    {
        await page.GotoAsync($"{rootUri}/Login");
        await page.WaitForSelectorAsync("#Email");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);

        await page.RunAndWaitForNavigationAsync(
            async () => await page.ClickAsync("button[type=submit]"),
            new PageRunAndWaitForNavigationOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 15_000
            });

        if (page.Url.Contains("/Login", StringComparison.OrdinalIgnoreCase))
        {
            var errors = await page.Locator(".text-danger, .validation-summary-errors").AllTextContentsAsync();
            throw new InvalidOperationException(
                $"Login failed for {email}. URL={page.Url}. Errors={string.Join(" | ", errors.Where(e => !string.IsNullOrWhiteSpace(e)))}");
        }
    }

    public static async Task RegisterOperatorAsync(IPage page, string rootUri, string email, string password)
    {
        await page.GotoAsync($"{rootUri}/Register/Operator",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.FillAsync("#FirstName", "Test");
        await page.FillAsync("#LastName", "Operator");
        await page.FillAsync("#Email", email);
        await page.FillAsync("#Password", password);
        await page.FillAsync("#ConfirmPassword", password);
        await page.FillAsync("#OperatorName", "TestOrg-" + Guid.NewGuid().ToString("N")[..8]);
        await page.SelectOptionAsync("#country-select", new SelectOptionValue { Value = "US" });
        await page.SelectOptionAsync("#OperatorType", new SelectOptionValue { Label = "Commercial" });

        await page.RunAndWaitForNavigationAsync(
            async () => await page.ClickAsync("button[type=submit]"),
            new PageRunAndWaitForNavigationOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 30_000
            });

        if (page.Url.Contains("/Register/Operator", StringComparison.OrdinalIgnoreCase))
        {
            var errors = await page.Locator(".text-danger, .validation-summary-errors").AllTextContentsAsync();
            throw new InvalidOperationException(
                $"Operator registration failed for {email}. URL={page.Url}. Errors={string.Join(" | ", errors.Where(e => !string.IsNullOrWhiteSpace(e)))}");
        }
    }

    public static async Task LogoutAsync(IPage page)
    {
        var logout = page.Locator("form[action='/Login/Logout'] button[type=submit]");
        if (await logout.CountAsync() > 0)
        {
            await logout.First.ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }

    public static async Task SetCultureAsync(IPage page, string rootUri, string culture)
    {
        await page.GotoAsync($"{rootUri}/Home/SetLanguage?culture={culture}&returnUrl=%2F",
            new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
    }
}