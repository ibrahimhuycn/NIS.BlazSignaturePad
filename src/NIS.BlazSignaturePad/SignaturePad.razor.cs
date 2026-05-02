using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace NIS.BlazSignaturePad;

public partial class SignaturePad : IAsyncDisposable
{
    [Parameter] public string? Id { get; set; }
    [Parameter] public SignaturePadOptions? Options { get; set; }
    [Parameter] public string Class { get; set; } = "";
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> Attributes { get; set; } = [];
    [Parameter] public EventCallback OnBeginStroke { get; set; }
    [Parameter] public EventCallback OnEndStroke { get; set; }
    [Parameter] public EventCallback<bool> IsEmptyChanged { get; set; }

    private string _id = "";
    private readonly DotNetObjectReference<SignaturePad> _dotNetRef;
    private IJSObjectReference? _module;

    public SignaturePad() => _dotNetRef = DotNetObjectReference.Create(this);

    protected override void OnInitialized()
    {
        _id = Id ?? Guid.NewGuid().ToString("n");
    }

    [JSInvokable]
    public async Task NotifyBeginStroke() => await OnBeginStroke.InvokeAsync();

    [JSInvokable]
    public async Task NotifyEndStroke() => await OnEndStroke.InvokeAsync();

    [JSInvokable]
    public async Task NotifyIsEmptyChanged(bool isEmpty) => await IsEmptyChanged.InvokeAsync(isEmpty);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/NIS.BlazSignaturePad/sigpad.interop.js?ver=5.1.3");
            var optionsJson = Options is null ? null : JsonSerializer.Serialize(Options, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
            await _module.InvokeVoidAsync("init", _id, _dotNetRef, optionsJson);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task ClearAsync()
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("clear", _id);
    }

    public async Task<bool> IsEmptyAsync()
    {
        if (_module is null) return true;
        return await _module.InvokeAsync<bool>("isEmpty", _id);
    }

    public async Task UndoAsync()
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("undo", _id);
    }

    public async Task RedoAsync()
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("redo", _id);
    }

    public async Task SetPenColorAsync(string color)
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("setPenColor", _id, color);
    }

    public async Task SetBackgroundColorAsync(string color)
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("setBackgroundColor", _id, color);
    }

    public async Task SetPenWidthAsync(float min, float max)
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("setPenWidth", _id, min, max);
    }

    public async Task<string?> ToDataUrlAsync(string type = "image/png", object? encoderOptions = null)
    {
        if (_module is null) return null;
        return await _module.InvokeAsync<string?>("toDataURL", _id, type, encoderOptions);
    }

    public async Task<byte[]?> ToByteArrayAsync(string type = "image/png", object? encoderOptions = null)
    {
        if (_module is null) return null;
        var jsStreamRef = await _module.InvokeAsync<IJSStreamReference?>("toByteArray", _id, type, encoderOptions);
        if (jsStreamRef is null) return null;
        await using var stream = await jsStreamRef.OpenReadStreamAsync(maxAllowedSize: 10_000_000);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    public async Task<Stream?> ToStreamAsync(string type = "image/png", object? encoderOptions = null)
    {
        if (_module is null) return null;
        var jsStreamRef = await _module.InvokeAsync<IJSStreamReference?>("toByteArray", _id, type, encoderOptions);
        if (jsStreamRef is null) return null;
        return await jsStreamRef.OpenReadStreamAsync(maxAllowedSize: 10_000_000);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("destroy", _id);
                await _module.DisposeAsync();
            }
            catch (Exception ex) when (ex is JSDisconnectedException || ex is OperationCanceledException)
            {
            }
        }
        _dotNetRef.Dispose();
    }
}
