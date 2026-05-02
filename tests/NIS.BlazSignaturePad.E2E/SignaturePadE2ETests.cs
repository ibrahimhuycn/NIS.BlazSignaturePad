using Microsoft.Playwright;
using Xunit;

namespace NIS.BlazSignaturePad.E2E;

public class SignaturePadE2ETests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
        _context = await _browser.NewContextAsync(new() { ViewportSize = new() { Width = 1280, Height = 720 } });
    }

    public async Task DisposeAsync()
    {
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    private async Task<IPage> OpenPageAsync()
    {
        var page = await _context.NewPageAsync();
        await page.GotoAsync("http://localhost:5000");
        return page;
    }

    private record Box(float X, float Y, float Width, float Height);

    private async Task<Box> GetCanvasBoxAsync(IPage page)
    {
        var canvas = page.Locator("#sig-signature-pad-demo");
        await canvas.WaitForAsync();
        for (int i = 0; i < 10; i++)
        {
            var box = await canvas.BoundingBoxAsync();
            if (box != null) return new Box(box.X, box.Y, box.Width, box.Height);
            await page.WaitForTimeoutAsync(100);
        }
        throw new InvalidOperationException("Canvas bounding box is null");
    }

    private async Task DrawOnCanvasAsync(IPage page)
    {
        var box = await GetCanvasBoxAsync(page);
        await page.Mouse.MoveAsync(box.X + 50, box.Y + 50);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(box.X + 200, box.Y + 100);
        await page.Mouse.MoveAsync(box.X + 350, box.Y + 50);
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(200);
    }

    private async Task DrawDotAsync(IPage page)
    {
        var box = await GetCanvasBoxAsync(page);
        await page.Mouse.MoveAsync(box.X + 100, box.Y + 100);
        await page.Mouse.DownAsync();
        await page.Mouse.UpAsync();
        await page.WaitForTimeoutAsync(200);
    }

    [Fact]
    public async Task Page_Loads_With_Canvas()
    {
        var page = await OpenPageAsync();
        var canvas = page.Locator("#sig-signature-pad-demo");
        await canvas.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        Assert.True(await canvas.IsVisibleAsync());
    }

    [Fact]
    public async Task Draw_Signature_Canvas_Not_Empty()
    {
        var page = await OpenPageAsync();
        await DrawOnCanvasAsync(page);

        var status = page.Locator(".alert");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save as PNG" }).ClickAsync();
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.Equal("PNG downloaded", text);
    }

    [Fact]
    public async Task Clear_Button_Clears_Canvas()
    {
        var page = await OpenPageAsync();
        await DrawOnCanvasAsync(page);

        await page.GetByRole(AriaRole.Button, new() { Name = "Clear" }).ClickAsync();
        await page.WaitForTimeoutAsync(200);

        await page.GetByRole(AriaRole.Button, new() { Name = "Save as PNG" }).ClickAsync();
        var status = page.Locator(".alert");
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.Equal("Please provide a signature first.", text);
    }

    [Fact]
    public async Task Undo_Button_Removes_Last_Stroke()
    {
        var page = await OpenPageAsync();
        await DrawOnCanvasAsync(page);
        await DrawDotAsync(page);

        await page.GetByRole(AriaRole.Button, new() { Name = "Undo" }).ClickAsync();
        await page.WaitForTimeoutAsync(200);

        var status = page.Locator(".alert");
        await page.GetByRole(AriaRole.Button, new() { Name = "Save as PNG" }).ClickAsync();
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.Equal("PNG downloaded", text);
    }

    [Fact]
    public async Task Redo_Button_Restores_Undone_Stroke()
    {
        var page = await OpenPageAsync();
        await DrawOnCanvasAsync(page);
        await DrawDotAsync(page);

        await page.GetByRole(AriaRole.Button, new() { Name = "Undo" }).ClickAsync();
        await page.WaitForTimeoutAsync(200);
        await page.GetByRole(AriaRole.Button, new() { Name = "Redo" }).ClickAsync();
        await page.WaitForTimeoutAsync(200);

        var downloadTask = page.WaitForDownloadAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save as PNG" }).ClickAsync();
        var download = await downloadTask;

        Assert.Contains("signature.png", download.SuggestedFilename);
        var path = await download.PathAsync();
        Assert.NotNull(path);
        var info = new FileInfo(path);
        Assert.True(info.Length > 100, $"PNG file too small: {info.Length} bytes");
    }

    [Fact]
    public async Task Change_Color_Button_Changes_Pen_Color()
    {
        var page = await OpenPageAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Change color" }).ClickAsync();

        var status = page.Locator(".alert");
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.NotNull(text);
        Assert.StartsWith("Color:", text);
    }

    [Fact]
    public async Task Change_Width_Button_Changes_Pen_Width()
    {
        var page = await OpenPageAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Change width" }).ClickAsync();

        var status = page.Locator(".alert");
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.NotNull(text);
        Assert.StartsWith("Width:", text);
    }

    [Fact]
    public async Task Change_Background_Button_Changes_Background()
    {
        var page = await OpenPageAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Change background" }).ClickAsync();

        var status = page.Locator(".alert");
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.NotNull(text);
        Assert.StartsWith("Background:", text);
    }

    [Fact]
    public async Task Transparent_Background_Button_Sets_Transparent()
    {
        var page = await OpenPageAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Transparent background" }).ClickAsync();

        var status = page.Locator(".alert");
        await status.WaitForAsync();
        var text = await status.TextContentAsync();
        Assert.Equal("Background: transparent", text);
    }

    [Fact]
    public async Task Save_Png_Button_Downloads_Non_Empty_File()
    {
        var page = await OpenPageAsync();
        await DrawOnCanvasAsync(page);

        var downloadTask = page.WaitForDownloadAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save as PNG" }).ClickAsync();
        var download = await downloadTask;

        Assert.Contains("signature.png", download.SuggestedFilename);
        var path = await download.PathAsync();
        Assert.NotNull(path);
        var info = new FileInfo(path);
        Assert.True(info.Length > 100, $"PNG file too small: {info.Length} bytes");
    }

    [Fact]
    public async Task Save_Svg_Button_Downloads_Valid_Svg()
    {
        var page = await OpenPageAsync();
        await DrawOnCanvasAsync(page);

        var downloadTask = page.WaitForDownloadAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save as SVG" }).ClickAsync();
        var download = await downloadTask;

        Assert.Contains("signature.svg", download.SuggestedFilename);
        var path = await download.PathAsync();
        Assert.NotNull(path);
        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("<svg", content);
        Assert.Contains("</svg>", content);
    }

    [Fact]
    public async Task Draw_Large_Signature_And_Save_Png()
    {
        var page = await OpenPageAsync();
        var box = await GetCanvasBoxAsync(page);

        for (int i = 0; i < 10; i++)
        {
            await page.Mouse.MoveAsync(box.X + 20 + i * 30, box.Y + 20 + i * 20);
            await page.Mouse.DownAsync();
            await page.Mouse.MoveAsync(box.X + 100 + i * 30, box.Y + 80 + i * 20);
            await page.Mouse.UpAsync();
        }
        await page.WaitForTimeoutAsync(500);

        var downloadTask = page.WaitForDownloadAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Save as PNG" }).ClickAsync();
        var download = await downloadTask;

        var path = await download.PathAsync();
        Assert.NotNull(path);
        var info = new FileInfo(path);
        Assert.True(info.Length > 1000, $"Large PNG file too small: {info.Length} bytes");
    }
}
