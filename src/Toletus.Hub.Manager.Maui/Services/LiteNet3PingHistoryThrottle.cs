using System.Collections.Concurrent;

namespace Toletus.Hub.Manager.Maui.Services;

internal sealed class LiteNet3PingHistoryThrottle(TimeSpan minimumInterval)
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastRecordedAt = new(StringComparer.OrdinalIgnoreCase);

    public bool ShouldRecord(string deviceKey, DateTimeOffset timestamp)
    {
        while (true)
        {
            if (!_lastRecordedAt.TryGetValue(deviceKey, out var lastRecorded))
                return _lastRecordedAt.TryAdd(deviceKey, timestamp);

            if (timestamp - lastRecorded < minimumInterval)
                return false;

            if (_lastRecordedAt.TryUpdate(deviceKey, timestamp, lastRecorded))
                return true;
        }
    }
}
