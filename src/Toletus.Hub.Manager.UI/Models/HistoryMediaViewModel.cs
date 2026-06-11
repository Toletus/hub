namespace Toletus.Hub.Manager.UI.Models;

public sealed record HistoryMediaViewModel(
    string Kind,
    string Source,
    string? AltKey = null,
    string? CaptionKey = null);
