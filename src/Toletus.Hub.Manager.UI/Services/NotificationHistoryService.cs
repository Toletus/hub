using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toletus.Hub.Manager.UI.Models;
using Toletus.Hub.Manager.UI.Contracts;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class NotificationHistoryService(
    ICommandHistoryFormatter formatter,
    ILogger<NotificationHistoryService> logger)
{
    private const int MaxItems = 100;
    private readonly List<CommandHistoryItemViewModel> _items = [];

    public event Action? Changed;

    public IReadOnlyList<CommandHistoryItemViewModel> Items => _items;

    public void Add(string commandId, CommandResultViewModel result)
    {
        var timestamp = Stopwatch.GetTimestamp();
        logger.LogInformation(
            "HubManagerDiag History.Add start command={CommandId} status={Status} device={DeviceKey} thread={ThreadId}",
            commandId,
            result.Status,
            result.Device?.Key,
            Environment.CurrentManagedThreadId);

        Add(formatter.Format(DateTimeOffset.Now, commandId, result));
        logger.LogInformation(
            "HubManagerDiag History.Add end command={CommandId} status={Status} elapsedMs={ElapsedMs} count={Count} thread={ThreadId}",
            commandId,
            result.Status,
            Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds,
            _items.Count,
            Environment.CurrentManagedThreadId);
    }

    public void Add(CommandHistoryItemViewModel item)
    {
        var timestamp = Stopwatch.GetTimestamp();
        _items.Insert(0, item);
        if (_items.Count > MaxItems)
            _items.RemoveRange(MaxItems, _items.Count - MaxItems);

        Changed?.Invoke();
        logger.LogInformation(
            "HubManagerDiag History.AddItem command={CommandId} status={Status} elapsedMs={ElapsedMs} count={Count} thread={ThreadId}",
            item.CommandId,
            item.Status,
            Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds,
            _items.Count,
            Environment.CurrentManagedThreadId);
    }

    public void Clear()
    {
        _items.Clear();
        Changed?.Invoke();
    }
}
