using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace CodeGenPlaywright;

[TestFixture]
public class LoginTests : PageTest
{
    private IConfiguration? _testSettings;
    
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _testSettings = new ConfigurationBuilder().AddJsonFile("testSettings.json").Build();
    }
    
    [TestCase]
    public async Task SuccessfulLoginTest()
    {
        ArgumentNullException.ThrowIfNull(_testSettings);
        await using var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {Headless = false});
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var baseUrl = _testSettings["baseUrl"] ?? throw new NullReferenceException("baseUrl is not defined in testsettings.json");
        var emailAddress = _testSettings["emailAddress"] ?? throw new NullReferenceException("emailAddress is not defined in testsettings.json");
        var password = _testSettings["password"] ?? throw new NullReferenceException("password is not defined in testsettings.json");
        
        await page.GotoAsync(baseUrl);
        
        await page.GetByRole(AriaRole.Link, new() { Name = " Signup / Login" }).ClickAsync();
        await page.Locator("form").Filter(new() { HasText = "Login" }).GetByPlaceholder("Email Address").FillAsync(emailAddress);
        await page.GetByPlaceholder("Password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        await Expect(page.GetByText("Logged in as Yumi Cheng")).ToBeVisibleAsync();
    }
    
    [TestCase]
    public async Task UnsuccessfulLoginTest()
    {
        ArgumentNullException.ThrowIfNull(_testSettings);
        await using var browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions {Headless = false});
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        var baseUrl = _testSettings["baseUrl"] ?? throw new NullReferenceException("baseUrl is not defined in testsettings.json");
        
        await page.GotoAsync(baseUrl);
        
        await page.GetByRole(AriaRole.Link, new() { Name = " Signup / Login" }).ClickAsync();
        await page.Locator("form").Filter(new() { HasText = "Login" }).GetByPlaceholder("Email Address").FillAsync("e@e");
        await page.GetByPlaceholder("Password").FillAsync("password");
        await page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();

        await Expect(page.Locator("#form")).ToContainTextAsync("Your email or password is incorrect!");
    }
}