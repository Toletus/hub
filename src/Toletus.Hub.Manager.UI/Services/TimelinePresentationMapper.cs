using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class TimelinePresentationMapper
{
    public TimelinePresentationViewModel Map(CommandHistoryItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var kind = GetKind(item);
        var severity = GetSeverity(item);
        return new TimelinePresentationViewModel
        {
            Kind = kind,
            Severity = severity,
            KindCssClass = kind switch
            {
                TimelineEventKind.Command => "tag-cmd",
                TimelineEventKind.Connection => "tag-resp",
                TimelineEventKind.Configuration => "tag-sys",
                TimelineEventKind.Discovery => "tag-sys",
                TimelineEventKind.Notification => "tag-evt",
                TimelineEventKind.Error => "tag-err",
                _ => "tag-cmd"
            },
            KindLabelKey = kind switch
            {
                TimelineEventKind.Command => "History.Kind.Command",
                TimelineEventKind.Connection => "History.Kind.Event",
                TimelineEventKind.Configuration => "History.Kind.System",
                TimelineEventKind.Discovery => "History.Kind.System",
                TimelineEventKind.Notification => "History.Kind.Event",
                TimelineEventKind.Error => "History.Kind.Error",
                _ => "History.Kind.Command"
            },
            SeverityCssClass = severity switch
            {
                TimelinePresentationSeverity.Idle => "sev-info",
                TimelinePresentationSeverity.Running => "sev-run",
                TimelinePresentationSeverity.Success => "sev-ok",
                TimelinePresentationSeverity.Warning => "sev-warn",
                TimelinePresentationSeverity.Error => "sev-err",
                _ => "sev-info"
            }
        };
    }

    private static TimelineEventKind GetKind(CommandHistoryItemViewModel item)
    {
        if (item.Status == CommandStatusKind.Error)
            return TimelineEventKind.Error;

        if (item.CommandId.StartsWith("connection.", StringComparison.OrdinalIgnoreCase))
            return TimelineEventKind.Connection;

        if (item.CommandId.StartsWith("configuration.", StringComparison.OrdinalIgnoreCase))
            return TimelineEventKind.Configuration;

        if (item.CommandId.Equals("discovery", StringComparison.OrdinalIgnoreCase))
            return TimelineEventKind.Discovery;

        if (item.CommandId.StartsWith("notification.", StringComparison.OrdinalIgnoreCase))
            return TimelineEventKind.Notification;

        return TimelineEventKind.Command;
    }

    private static TimelinePresentationSeverity GetSeverity(CommandHistoryItemViewModel item) =>
        item.Status switch
        {
            CommandStatusKind.Pending => TimelinePresentationSeverity.Running,
            CommandStatusKind.Success => TimelinePresentationSeverity.Success,
            CommandStatusKind.Warning => TimelinePresentationSeverity.Warning,
            CommandStatusKind.Error => TimelinePresentationSeverity.Error,
            _ => TimelinePresentationSeverity.Idle
        };
}
