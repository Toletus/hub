using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class HubManagerThemeState
{
    private const string DefaultAccent = "#F9760D";
    private string _accent = DefaultAccent;
    private HubManagerDensity _density = HubManagerDensity.Regular;
    private HubManagerTimeFormat _timeFormat = HubManagerTimeFormat.Clock;

    public event Action? Changed;

    public string Accent
    {
        get => _accent;
        set
        {
            var next = string.IsNullOrWhiteSpace(value) ? DefaultAccent : value;
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
}
