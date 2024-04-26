using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace CodeGenPlaywright;

[TestFixture]
public class JSLoginTest : PageTest
{
    private IConfiguration? _testSettings;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _testSettings = new ConfigurationBuilder().AddJsonFile("testSettingsJS.json").Build();
    }

    [TestCase]
    public async Task SuccessfulLoginTest()
    {
        ArgumentNullException.ThrowIfNull(_testSettings);
        await using var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var baseUrl = _testSettings["baseUrl"] ?? throw new NullReferenceException("baseUrl is not defined in testsettings.json");
        var emailAddress = _testSettings["emailAddress"] ?? throw new NullReferenceException("emailAddress is not defined in testsettings.json");
        var password = _testSettings["password"] ?? throw new NullReferenceException("password is not defined in testsettings.json");
        
        await page.GotoAsync($"{baseUrl}login");
        
        await page.GetByLabel("Close Welcome Banner").ClickAsync();
        await page.GetByLabel("Text field for the login email").FillAsync(emailAddress);
        await page.GetByLabel("Text field for the login password").FillAsync(password);
        await page.GetByLabel("Login", new() { Exact = true }).ClickAsync();
        await page.GetByLabel("Show/hide account menu").ClickAsync();
        await Expect(page.Locator("#mat-menu-panel-0")).ToContainTextAsync("yumi@hello.com");
    }
    
    [TestCase]
    public async Task UnsuccessfulLoginTest()
    {
        ArgumentNullException.ThrowIfNull(_testSettings);
        await using var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {Headless = false});
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var baseUrl = _testSettings["baseUrl"] ?? throw new NullReferenceException("baseUrl is not defined in testsettings.json");
        
        await page.GotoAsync($"{baseUrl}login");

        await page.GetByLabel("Close Welcome Banner").ClickAsync();
        await page.GetByLabel("Text field for the login email").FillAsync("who");
        await page.GetByLabel("Text field for the login password").FillAsync("who");
        await page.GetByLabel("Login", new() { Exact = true }).ClickAsync();
        await Expect(page.Locator("mat-card")).ToContainTextAsync("Invalid email or password.");
    }
    
    [TestCase]
    public async Task UnsuccessfulSQLInjectionLoginTest()
    {
        ArgumentNullException.ThrowIfNull(_testSettings);
        await using var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {Headless = false});
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var baseUrl = _testSettings["baseUrl"] ?? throw new NullReferenceException("baseUrl is not defined in testsettings.json");
        
        await page.GotoAsync($"{baseUrl}login");

        await page.GetByLabel("Close Welcome Banner").ClickAsync();
        await page.GetByLabel("Text field for the login email").FillAsync("' or 1=1 --");
        await page.GetByLabel("Text field for the login password").FillAsync("anything");
        await page.GetByLabel("Login", new() { Exact = true }).ClickAsync();
        await Expect(page.Locator("mat-card")).ToContainTextAsync("Invalid email or password.");
    }
}