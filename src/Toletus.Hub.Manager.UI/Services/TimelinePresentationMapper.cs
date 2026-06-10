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
                TimelineEventKind.Connection => "tag-conn",
                TimelineEventKind.Configuration => "tag-config",
                TimelineEventKind.Discovery => "tag-discovery",
                TimelineEventKind.Notification => "tag-note",
                TimelineEventKind.Error => "tag-error",
                _ => "tag-cmd"
            },
            SeverityCssClass = severity switch
            {
                TimelinePresentationSeverity.Idle => "sev-idle",
                TimelinePresentationSeverity.Running => "sev-run",
                TimelinePresentationSeverity.Success => "sev-ok",
                TimelinePresentationSeverity.Warning => "sev-warn",
                TimelinePresentationSeverity.Error => "sev-error",
                _ => "sev-idle"
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
