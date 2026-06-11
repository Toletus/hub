using System.Collections.Concurrent;

namespace Toletus.Hub.Manager.Maui.Services;

internal sealed class ReleaseCommandGate
{
    private readonly ConcurrentDictionary<string, string> _activeCommands = new(StringComparer.OrdinalIgnoreCase);

    public bool TryEnter(string deviceKey, string commandId, out string? activeCommandId)
    {
        if (_activeCommands.TryAdd(deviceKey, commandId))
        {
            activeCommandId = null;
            return true;
        }

        _activeCommands.TryGetValue(deviceKey, out activeCommandId);
        return false;
    }

    public void Exit(string deviceKey)
    {
        _activeCommands.TryRemove(deviceKey, out _);
    }
}
