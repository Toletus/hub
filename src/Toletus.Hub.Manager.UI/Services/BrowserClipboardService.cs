using Microsoft.JSInterop;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class BrowserClipboardService(IJSRuntime jsRuntime) : IClipboardService, IAsyncDisposable
{
    private const string ModulePath = "_content/Toletus.Hub.Manager.UI/hubManagerClipboard.js";
    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath).AsTask());

    public async Task<bool> CopyTextAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<bool>("copyText", cancellationToken, text);
        }
        catch (JSException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!moduleTask.IsValueCreated)
            return;

        try
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}
