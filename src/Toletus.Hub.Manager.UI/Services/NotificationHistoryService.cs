using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class NotificationHistoryService
{
    private readonly List<CommandHistoryItemViewModel> _items = [];

    public event Action? Changed;

    public IReadOnlyList<CommandHistoryItemViewModel> Items => _items;

    public void Add(string commandId, CommandResultViewModel result)
    {
        _items.Insert(0, new CommandHistoryItemViewModel
        {
            Timestamp = DateTimeOffset.Now,
            CommandId = commandId,
            Status = result.Status,
            MessageKey = result.MessageKey,
            DeviceName = result.Device?.Name,
            TechnicalDetails = result.TechnicalDetails
        });
        Changed?.Invoke();
    }

    public void Clear()
    {
        _items.Clear();
        Changed?.Invoke();
    }
}
