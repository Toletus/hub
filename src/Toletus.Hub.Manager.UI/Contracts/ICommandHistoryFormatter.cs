using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Contracts;

public interface ICommandHistoryFormatter
{
    CommandHistoryItemViewModel Format(
        DateTimeOffset timestamp,
        string commandId,
        CommandResultViewModel result);
}
