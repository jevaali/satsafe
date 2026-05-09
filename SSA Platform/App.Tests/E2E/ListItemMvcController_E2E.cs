using App.Tests.E2E;
using App.Tests.Integration.Helpers;
using AwesomeAssertions;
using Microsoft.Playwright;


namespace WebApp.Tests.E2E
{
    [Trait("Category", "E2E")]
    [Collection("E2E")]
    public class ListItemMvcController_E2E
    {
        private readonly PlaywrightWebAppFactory _factory;

        public ListItemMvcController_E2E(PlaywrightWebAppFactory factory)
        {
            _factory = factory;
        }

        private static string FreshEmail(string label) =>
            $"{label}+{Guid.NewGuid():N}@test.ee";

        private async Task<(IPlaywright pw, IBrowser browser)> NewBrowserAsync()
        {
            var pw = await Playwright.CreateAsync();
            var browser = await pw.Chromium.ConnectAsync("ws://localhost:3000/");
            return (pw, browser);
        }

        private async Task<IPage> NewPageAsync(IBrowser browser) =>
            await (await browser.NewContextAsync()).NewPageAsync();

        [Fact]
        public async Task Index_Anonymous_RedirectsToLogin()
        {
            var (pw, browser) = await NewBrowserAsync();
            try
            {
                var page = await NewPageAsync(browser);
                await page.GotoAsync($"{_factory.RootUri}/ListItems");
                page.Url.Should().Contain("/Identity/Account/Login");
            }
            finally
            {
                await browser.DisposeAsync();
                pw.Dispose();
            }
        }

        [Fact]
        public async Task Index_AsRegisteredUser_ShowsOnlyOwnItems()
        {
            var (pw, browser) = await NewBrowserAsync();
            try
            {
                var emailA = FreshEmail("user-a");
                var emailB = FreshEmail("user-b");

                // Register UserA, create an item, logout
                var pageA = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(pageA, _factory.RootUri, emailA, TestUsers.Password);
                await PlaywrightLoginHelper.SetCultureAsync(pageA, _factory.RootUri, "en");
                await CreateItemAsync(pageA, "userA-item", "userA-summary");

                // Register UserB, create an item
                var pageB = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(pageB, _factory.RootUri, emailB, TestUsers.Password);
                await PlaywrightLoginHelper.SetCultureAsync(pageB, _factory.RootUri, "en");
                await CreateItemAsync(pageB, "userB-item", "userB-summary");

                // Verify A's index shows only A's items (NOT B's)
                await pageA.GotoAsync($"{_factory.RootUri}/ListItems");
                var bodyA = await pageA.Locator("body").TextContentAsync();
                bodyA.Should().Contain("userA-item");
                bodyA.Should().NotContain("userB-item");
            }
            finally
            {
                await browser.DisposeAsync();
                pw.Dispose();
            }
        }

        [Fact]
        public async Task Create_Edit_Details_Delete_RoundTrip()
        {
            var (pw, browser) = await NewBrowserAsync();
            try
            {
                var page = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(page, _factory.RootUri,
                    FreshEmail("user-crud"), TestUsers.Password);
                await PlaywrightLoginHelper.SetCultureAsync(page, _factory.RootUri, "en");

                var description = $"e2e-{Guid.NewGuid():N}";
                await CreateItemAsync(page, description, "summary-1");
                (await page.Locator("body").TextContentAsync()).Should().Contain(description);

                // Edit — find the row containing the description, click its Edit link
                await ClickRowActionAsync(page, description, "Edit");
                await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/ListItems/Edit/.*"));
                await page.FillAsync("#Summary", "summary-1-updated");
                await page.ClickAsync("input[type=submit]");
                await page.WaitForURLAsync($"{_factory.RootUri}/ListItems");
                (await page.Locator("body").TextContentAsync()).Should().Contain("summary-1-updated");

                // Details
                await ClickRowActionAsync(page, description, "Details");
                await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/ListItems/Details/.*"));
                (await page.Locator("body").TextContentAsync()).Should().Contain(description);

                // Delete (confirmation page → confirm)
                await page.GotoAsync($"{_factory.RootUri}/ListItems");
                await ClickRowActionAsync(page, description, "Delete");
                await page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(".*/ListItems/Delete/.*"));
                await page.ClickAsync("input[type=submit]");
                await page.WaitForURLAsync($"{_factory.RootUri}/ListItems");
                (await page.Locator("body").TextContentAsync()).Should().NotContain(description);
            }
            finally
            {
                await browser.DisposeAsync();
                pw.Dispose();
            }
        }

        private static async Task ClickRowActionAsync(IPage page, string description, string action)
        {
            var row = page.Locator("tr").Filter(new() { HasTextString = description });
            await row.Locator($"a:has-text('{action}')").First.ClickAsync();
        }

        [Fact]
        public async Task Edit_ForeignItemId_Returns404_IDOR()
        {
            var (pw, browser) = await NewBrowserAsync();
            try
            {
                var pageB = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(pageB, _factory.RootUri,
                    FreshEmail("user-idor-b"), TestUsers.Password);
                await PlaywrightLoginHelper.SetCultureAsync(pageB, _factory.RootUri, "en");
                var bDescription = $"b-item-{Guid.NewGuid():N}";
                await CreateItemAsync(pageB, bDescription, "b-summary");

                // Capture B's item id from the Edit link on B's index page
                var bItemId = await ExtractItemIdFromIndexAsync(pageB, bDescription);

                // UserA registers and tries to edit UserB's item by id
                var pageA = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(pageA, _factory.RootUri,
                    FreshEmail("user-idor-a"), TestUsers.Password);
                var resp = await pageA.GotoAsync($"{_factory.RootUri}/ListItems/Edit/{bItemId}");
                resp!.Status.Should().Be(404);
            }
            finally
            {
                await browser.DisposeAsync();
                pw.Dispose();
            }
        }

        [Fact]
        public async Task Delete_ForeignItemId_Returns404_IDOR()
        {
            var (pw, browser) = await NewBrowserAsync();
            try
            {
                var pageB = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(pageB, _factory.RootUri,
                    FreshEmail("user-del-b"), TestUsers.Password);
                await PlaywrightLoginHelper.SetCultureAsync(pageB, _factory.RootUri, "en");
                var bDescription = $"b-del-{Guid.NewGuid():N}";
                await CreateItemAsync(pageB, bDescription, "b-summary");

                var bItemId = await ExtractItemIdFromIndexAsync(pageB, bDescription);

                var pageA = await NewPageAsync(browser);
                await PlaywrightLoginHelper.RegisterAsync(pageA, _factory.RootUri,
                    FreshEmail("user-del-a"), TestUsers.Password);
                var resp = await pageA.GotoAsync($"{_factory.RootUri}/ListItems/Delete/{bItemId}");
                resp!.Status.Should().Be(404);
            }
            finally
            {
                await browser.DisposeAsync();
                pw.Dispose();
            }
        }

        private static async Task<Guid> ExtractItemIdFromIndexAsync(IPage page, string description)
        {
            await page.GotoAsync(GetRoot(page) + "/ListItems");
            var row = page.Locator("tr").Filter(new() { HasTextString = description });
            var href = await row.Locator("a:has-text('Edit')").First.GetAttributeAsync("href");
            if (href == null) throw new InvalidOperationException($"Edit link not found for {description}");
            var idStr = href.Substring(href.LastIndexOf('/') + 1);
            return Guid.Parse(idStr);
        }

        private static async Task CreateItemAsync(IPage page, string description, string summary)
        {
            await page.GotoAsync(GetRoot(page) + "/ListItems/Create");
            await page.FillAsync("#ItemDescription", description);
            await page.FillAsync("#Summary", summary);
            await page.ClickAsync("input[type=submit]");
            await page.WaitForURLAsync(GetRoot(page) + "/ListItems");
        }

        private static string GetRoot(IPage page) =>
            new Uri(page.Url).GetLeftPart(UriPartial.Authority);
    }
}
