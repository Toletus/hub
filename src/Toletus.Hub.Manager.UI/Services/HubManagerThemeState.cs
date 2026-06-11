using Microsoft.JSInterop;
using System.Text.Json;
using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class HubManagerThemeState
{
    private const string DefaultAccent = "#F9760D";
    private const string StorageKey = "toletus.hub.manager.theme";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private string _accent = DefaultAccent;
    private HubManagerDensity _density = HubManagerDensity.Regular;
    private HubManagerTimeFormat _timeFormat = HubManagerTimeFormat.Clock;

    public event Action? Changed;

    public string Accent
    {
        get => _accent;
        set
        {
            var next = NormalizeAccent(value);
            if (string.Equals(_accent, next, StringComparison.Ordinal))
                return;

            _accent = next;
            Changed?.Invoke();
        }
    }

    public HubManagerDensity Density
    {
        get => _density;
        set
        {
            if (_density == value)
                return;

            _density = value;
            Changed?.Invoke();
        }
    }

    public HubManagerTimeFormat TimeFormat
    {
        get => _timeFormat;
        set
        {
            if (_timeFormat == value)
                return;

            _timeFormat = value;
            Changed?.Invoke();
        }
    }

    public async Task LoadAsync(IJSRuntime jsRuntime, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
            if (string.IsNullOrWhiteSpace(payload))
                return;

            var persisted = JsonSerializer.Deserialize<PersistedThemeState>(payload, JsonOptions);
            if (persisted is null)
                return;

            var hasChanged = false;
            if (!string.IsNullOrWhiteSpace(persisted.Accent))
            {
                var nextAccent = NormalizeAccent(persisted.Accent);
                hasChanged |= !string.Equals(_accent, nextAccent, StringComparison.Ordinal);
                _accent = nextAccent;
            }

            if (Enum.TryParse<HubManagerDensity>(persisted.Density, ignoreCase: true, out var density))
            {
                hasChanged |= _density != density;
                _density = density;
            }

            if (Enum.TryParse<HubManagerTimeFormat>(persisted.TimeFormat, ignoreCase: true, out var timeFormat))
            {
                hasChanged |= _timeFormat != timeFormat;
                _timeFormat = timeFormat;
            }

            if (hasChanged)
                Changed?.Invoke();
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (JsonException)
        {
        }
    }

    public async Task SaveAsync(IJSRuntime jsRuntime, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = JsonSerializer.Serialize(
                new PersistedThemeState(_accent, _density.ToString(), _timeFormat.ToString()),
                JsonOptions);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, payload);
        }
        catch (JSException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static string NormalizeAccent(string value)
    {
        var cleaned = value.Trim().TrimStart('#');
        return cleaned.Length == 6 && cleaned.All(Uri.IsHexDigit)
            ? $"#{cleaned.ToUpperInvariant()}"
            : DefaultAccent;
    }

    private sealed record PersistedThemeState(string? Accent, string? Density, string? TimeFormat);
}
