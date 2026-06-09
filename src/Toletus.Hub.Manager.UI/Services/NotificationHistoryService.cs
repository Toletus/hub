using Toletus.Hub.Manager.UI.Models;
using Toletus.Hub.Manager.UI.Contracts;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class NotificationHistoryService(ICommandHistoryFormatter formatter)
{
    private const int MaxItems = 100;
    private readonly List<CommandHistoryItemViewModel> _items = [];

    public event Action? Changed;

    public IReadOnlyList<CommandHistoryItemViewModel> Items => _items;

    public void Add(string commandId, CommandResultViewModel result)
    {
        Add(formatter.Format(DateTimeOffset.Now, commandId, result));
    }

    public void Add(CommandHistoryItemViewModel item)
    {
        _items.Insert(0, item);
        if (_items.Count > MaxItems)
            _items.RemoveRange(MaxItems, _items.Count - MaxItems);

        Changed?.Invoke();
    }

    public void Clear()
    {
        _items.Clear();
        Changed?.Invoke();
    }
}
