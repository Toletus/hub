using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Capabilities;

public sealed class DeviceCapabilityCatalog
{
    public const string Connect = "connection.connect";
    public const string Disconnect = "connection.disconnect";
    public const string ReleaseEntry = "common.release_entry";
    public const string ReleaseEntryAndExit = "common.release_entry_exit";
    public const string ReleaseExit = "common.release_exit";
    public const string Reset = "common.reset";
    public const string ResetCounters = "common.reset_counters";
    public const string GetStatus = "common.get_status";
    public const string Sm25ReaderInfo = "sm25.reader_info";
    public const string Sm25Cancel = "sm25.cancel";

    private static readonly CommandCapability[] Common =
    [
        new(Connect, "Command.Connect", "Group.Connection", RequiresConnection: false),
        new(Disconnect, "Command.Disconnect", "Group.Connection"),
        new(ReleaseEntry, "Command.ReleaseEntry", "Group.Operation"),
        new(ReleaseExit, "Command.ReleaseExit", "Group.Operation"),
        new(Reset, "Command.Reset", "Group.Maintenance", RequiresConfirmation: true),
        new(GetStatus, "Command.GetStatus", "Group.Diagnostics")
    ];

    public IReadOnlyList<CommandCapability> GetCommands(DeviceTypeKind type)
    {
        var commands = Common.ToList();

        if (type is DeviceTypeKind.LiteNet2 or DeviceTypeKind.LiteNet3)
            commands.Insert(4, new CommandCapability(ReleaseEntryAndExit, "Command.ReleaseEntryAndExit", "Group.Operation"));

        if (type is DeviceTypeKind.LiteNet1 or DeviceTypeKind.LiteNet2)
            commands.Add(new CommandCapability(ResetCounters, "Command.ResetCounters", "Group.Maintenance", RequiresConfirmation: true));

        return commands;
    }

    public IReadOnlyList<CommandCapability> GetModuleCommands(DeviceModuleKind module)
    {
        return module switch
        {
            DeviceModuleKind.SM25 =>
            [
                new(Sm25ReaderInfo, "Command.Sm25ReaderInfo", "Group.Biometrics"),
                new(Sm25Cancel, "Command.Sm25Cancel", "Group.Biometrics", RequiresConnection: false)
            ],
            _ => Array.Empty<CommandCapability>()
        };
    }

    public IReadOnlyList<ConfigurationFieldCapability> GetConfigurationFields(DeviceTypeKind type)
    {
        var fields = new List<ConfigurationFieldCapability>
        {
            new("firmware_version", "Field.FirmwareVersion", "Config.General", "read"),
            new("device_id", "Field.DeviceId", "Config.General", "number"),
            new("buzzer_mute", "Field.BuzzerMute", "Config.Operation", "boolean_select"),
            new("entered", "Field.Entered", "Config.Counters", "read"),
            new("exited", "Field.Exited", "Config.Counters", "read"),
            new("release_duration", "Field.ReleaseDuration", "Config.Operation", "number")
        };

        if (type is not DeviceTypeKind.LiteNet3)
            fields.Add(new("flow_mode", "Field.FlowMode", "Config.Flow", "select"));

        if (type is DeviceTypeKind.LiteNet1 or DeviceTypeKind.LiteNet2)
        {
            fields.AddRange([
                new("entry_clockwise", "Field.EntryClockwise", "Config.Operation", "boolean_select"),
                new("default_message", "Field.DefaultMessage", "Config.Messages", "text", "Placeholder.DefaultMessage"),
                new("secondary_message", "Field.SecondaryMessage", "Config.Messages", "text", "Placeholder.SecondaryMessage"),
                new("show_clock", "Field.ShowClock", "Config.Counters", "boolean_select"),
                new("show_counters", "Field.ShowCounters", "Config.Counters", "toggle")
            ]);
        }

        if (type is DeviceTypeKind.LiteNet2)
        {
            fields.AddRange([
                new("fingerprint_identification_mode", "Field.FingerprintMode", "Config.Biometrics", "select"),
                new("notify_color", "Field.NotifyColor", "Config.Notification", "text"),
                new("notify_tone", "Field.NotifyTone", "Config.Notification", "select"),
                new("notify_show_message", "Field.NotifyShowMessage", "Config.Notification", "boolean_select")
            ]);
        }

        if (type is DeviceTypeKind.LiteNet3)
        {
            fields.AddRange([
                new("alias", "Field.Alias", "Config.General", "text"),
                new("l3_flow_inverted", "Field.FlowInverted", "Config.Flow", "boolean_select"),
                new("l3_flow_in", "Field.FlowIn", "Config.Flow", "select"),
                new("l3_flow_out", "Field.FlowOut", "Config.Flow", "select"),
                new("l3_flow_front_wait", "Field.LedWait", "Config.Flow", "color"),
                new("l3_flow_picto_wait_in", "Field.PictoWaitIn", "Config.Flow", "select"),
                new("l3_flow_picto_wait_out", "Field.PictoWaitOut", "Config.Flow", "select"),
                new("default_message", "Field.DisplayTop", "Config.Messages", "text", "Placeholder.DefaultMessage"),
                new("secondary_message", "Field.DisplayBottom", "Config.Messages", "text", "Placeholder.SecondaryMessage"),
                new("display_mode", "Field.DisplayMode", "Config.Messages", "select")
            ]);
        }

        if (type is DeviceTypeKind.LiteNet2 or DeviceTypeKind.LiteNet3)
        {
            fields.AddRange([
                new("ip_mode", "Field.IpMode", "Config.Network", "select"),
                new("static_ip", "Field.StaticIp", "Config.Network", "ip"),
                new("subnet_mask", "Field.SubnetMask", "Config.Network", "ip"),
                new("menu_password", "Field.MenuPassword", "Config.Security", "password"),
                new("mac", "Field.Mac", "Config.Network", "text")
            ]);

            if (type is DeviceTypeKind.LiteNet3)
                fields.Add(new("gateway", "Field.Gateway", "Config.Network", "ip"));
        }

        return fields;
    }
}
