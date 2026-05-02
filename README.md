# NIS.BlazSignaturePad

A Blazor Razor Class Library (RCL) wrapping **[szimek/signature_pad](https://github.com/szimek/signature_pad)** v5.1.3 for smooth signature drawing in Blazor applications.

## Features

- Smooth signature drawing on HTML5 canvas
- PNG, JPEG, SVG export (`byte[]`, `Stream`, or data URL)
- Undo / Redo support
- Pen color, background color, and stroke width control
- Transparent background by default
- Streaming image export via `IJSStreamReference` (no SignalR size limits)
- Works in Blazor Server, WebAssembly, and SSR with interactive render modes

## Installation

```bash
dotnet add package NIS.BlazSignaturePad
```

## Quick Start — Blazor Server (Full Interactive)

### 1. Register Interactive Server Render Mode

In your Blazor Server app's `Program.cs`, ensure interactive server components are enabled:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Required for RCL static assets in non-Development environments
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

> **Important:** `builder.WebHost.UseStaticWebAssets()` is required so the browser can load the embedded JS files from the RCL. This is automatically enabled only in `Development`.

### 2. Add Global Interactive Render Mode

In `App.razor` (or your root layout), apply interactive render mode globally:

```razor
<!DOCTYPE html>
<html>
<head>
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

Or apply per-page:

```razor
@page "/sign"
@rendermode InteractiveServer
```

### 3. Use the Component

```razor
@using NIS.BlazSignaturePad

<SignaturePad @ref="_pad"
              Id="my-signature"
              Options="_options"
              style="width:100%;height:300px;"
              OnEndStroke="OnStrokeEnd" />

<button @onclick="SavePng">Save PNG</button>
<button @onclick="Clear">Clear</button>
<button @onclick="Undo">Undo</button>

@code {
    private SignaturePad _pad = null!;
    private SignaturePadOptions _options = new()
    {
        BackgroundColor = "rgb(255,255,255)",
        PenColor = "black",
        MinWidth = 0.5f,
        MaxWidth = 2.5f
    };

    private void OnStrokeEnd() { }

    private async Task SavePng()
    {
        if (await _pad.IsEmptyAsync()) return;
        var bytes = await _pad.ToByteArrayAsync("image/png");
        // save bytes...
    }

    private async Task Clear() => await _pad.ClearAsync();
    private async Task Undo() => await _pad.UndoAsync();
}
```

## API Reference

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Id` | `string?` | Custom element id. Canvas renders as `sig-{Id}`. |
| `Options` | `SignaturePadOptions?` | Pen color, width, background, etc. |
| `Class` | `string` | CSS class for `<canvas>`. |
| `Attributes` | `Dictionary<string, object>` | Additional HTML attributes splat. |
| `OnBeginStroke` | `EventCallback` | Fired when user starts drawing. |
| `OnEndStroke` | `EventCallback` | Fired when user finishes a stroke. |
| `IsEmptyChanged` | `EventCallback<bool>` | Fired when empty state changes. |

### Public Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ClearAsync()` | `Task` | Clears canvas and resets undo/redo. |
| `IsEmptyAsync()` | `Task<bool>` | True if canvas has no drawing. |
| `UndoAsync()` | `Task` | Removes last stroke. |
| `RedoAsync()` | `Task` | Restores last undone stroke. |
| `SetPenColorAsync(string)` | `Task` | Changes pen color. |
| `SetBackgroundColorAsync(string)` | `Task` | Changes background and redraws. |
| `SetPenWidthAsync(float min, float max)` | `Task` | Changes stroke width range. |
| `ToDataUrlAsync(string type, object? opts)` | `Task<string?>` | Returns data URL. |
| `ToByteArrayAsync(string type, object? opts)` | `Task<byte[]?>` | Returns image bytes via stream. |
| `ToStreamAsync(string type, object? opts)` | `Task<Stream?>` | Returns readable stream. |

### Supported Export Types

- `image/png` (default)
- `image/jpeg`
- `image/svg+xml`

## Options

```csharp
new SignaturePadOptions
{
    PenColor = "black",
    BackgroundColor = "rgba(0,0,0,0)",  // transparent default
    MinWidth = 0.5f,
    MaxWidth = 2.5f,
    VelocityFilterWeight = 0.7f,
    DotSize = 0,
    MinDistance = 5,
    Throttle = 16,
    CompositeOperation = "source-over"
};
```

## Samples

See `samples/BlazorServerSample/` for a full working demo with buttons for clear, undo, redo, color/width/background changes, and PNG/SVG export.

## Wrapped Library

This component wraps [signature_pad](https://github.com/szimek/signature_pad) by Szymon Nowak, distributed under the MIT license.

## License

MIT
