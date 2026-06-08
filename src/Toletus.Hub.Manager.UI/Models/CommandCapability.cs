namespace Toletus.Hub.Manager.UI.Models;

public sealed record CommandCapability(
    string Id,
    string LabelKey,
    string GroupKey,
    bool RequiresConnection = true,
    bool RequiresConfirmation = false,
    bool IsWebhook = false);
