using System.Collections;
using System.Text.Json;
using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Models;

namespace Toletus.Hub.Manager.UI.Services;

public class CommandHistoryFormatter : ICommandHistoryFormatter
{
    private const int MaxDepth = 3;
    private const int MaxDetails = 12;
    private static readonly JsonSerializerOptions PayloadJsonOptions = new(JsonSerializerDefaults.Web)
    {
        MaxDepth = 16
    };

    private static readonly Dictionary<string, string> CommandEventKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["discovery"] = "History.Event.Discovery",
        ["connection.connect"] = "History.Event.Connect",
        ["connection.disconnect"] = "History.Event.Disconnect",
        ["connection.connect_serial"] = "History.Event.ConnectSerial",
        ["common.release_entry"] = "History.Event.ReleaseEntry",
        ["common.release_entry_exit"] = "History.Event.ReleaseEntryAndExit",
        ["common.release_exit"] = "History.Event.ReleaseExit",
        ["common.reset"] = "History.Event.Reset",
        ["common.reset_counters"] = "History.Event.ResetCounters",
        ["common.get_status"] = "History.Event.Status",
        ["configuration.refresh"] = "History.Event.ConfigurationRefresh",
        ["sm25.reader_info"] = "History.Event.Sm25ReaderInfo",
        ["sm25.cancel"] = "History.Event.Sm25Cancel",
        ["notification.litenet3.biometrics"] = "History.Event.Biometrics",
        ["notification.identification"] = "History.Event.Identification",
        ["notification.identification.rfid"] = "History.Event.IdentificationRfid",
        ["notification.identification.barcode"] = "History.Event.IdentificationBarcode",
        ["notification.identification.keypad"] = "History.Event.IdentificationKeypad",
        ["notification.identification.fingerprint"] = "History.Event.IdentificationFingerprint",
        ["notification.passage"] = "History.Event.Passage",
        ["notification.timeout"] = "History.Event.Timeout",
        ["notification.status"] = "History.Event.NotificationStatus",
        ["notification.ready"] = "History.Event.Ready",
        ["notification.connection"] = "History.Event.ConnectionChanged",
        ["notification.fingerprint_reader"] = "History.Event.FingerprintReader",
        ["notification.sm25"] = "History.Event.Sm25",
        ["notification.litenet3.ping"] = "History.Event.PingResponse",
        ["notification.litenet3.error"] = "History.Event.ErrorResponse"
    };

    private static readonly Dictionary<string, string> CommandLabelKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["connection.connect"] = "Command.Connect",
        ["connection.disconnect"] = "Command.Disconnect",
        ["connection.connect_serial"] = "Command.ConnectSerial",
        ["common.release_entry"] = "Command.ReleaseEntry",
        ["common.release_entry_exit"] = "Command.ReleaseEntryAndExit",
        ["common.release_exit"] = "Command.ReleaseExit",
        ["common.reset"] = "Command.Reset",
        ["common.reset_counters"] = "Command.ResetCounters",
        ["common.get_status"] = "Command.GetStatus",
        ["configuration.refresh"] = "Command.GetProperties",
        ["sm25.reader_info"] = "Command.Sm25ReaderInfo",
        ["sm25.cancel"] = "Command.Sm25Cancel"
    };

    private static readonly Dictionary<string, string> DetailLabelKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Action"] = "History.Detail.Action",
        ["Barcode"] = "History.Detail.Barcode",
        ["Biometrics"] = "History.Detail.Biometrics",
        ["BoardConnectionStatus"] = "History.Detail.Connection",
        ["BottomRow"] = "History.Detail.SecondaryMessage",
        ["ChecksumIsValid"] = "History.Detail.Checksum",
        ["Code"] = "History.Detail.Code",
        ["Connected"] = "History.Detail.Connected",
        ["Content"] = "History.Detail.Content",
        ["content"] = "History.Detail.Content",
        ["ControlledFlow"] = "History.Detail.Flow",
        ["ControlledFlowExtended"] = "History.Detail.Flow",
        ["Data"] = "History.Detail.Content",
        ["DefaultMessage"] = "History.Detail.DefaultMessage",
        ["EntryClockwise"] = "History.Detail.EntryClockwise",
        ["Error"] = "History.Detail.Error",
        ["Exited"] = "History.Detail.Exited",
        ["Entered"] = "History.Detail.Entered",
        ["Firmware"] = "History.Detail.Firmware",
        ["FirmwareVersion"] = "History.Detail.Firmware",
        ["FingerprintIdentificationMode"] = "History.Detail.FingerprintMode",
        ["FingerprintReaderConnected"] = "History.Detail.FingerprintReader",
        ["FrontWait"] = "History.Detail.Flow",
        ["Gateway"] = "History.Detail.Gateway",
        ["Gyre"] = "History.Detail.Passage",
        ["Id"] = "History.Detail.Id",
        ["ID"] = "History.Detail.Id",
        ["Identification"] = "History.Detail.Identification",
        ["In"] = "History.Detail.Flow",
        ["Inverted"] = "History.Detail.Flow",
        ["Ip"] = "History.Detail.Ip",
        ["IP"] = "History.Detail.Ip",
        ["IpConfig"] = "History.Detail.IpMode",
        ["IpMode"] = "History.Detail.IpMode",
        ["IsReady"] = "History.Detail.Ready",
        ["Keypad"] = "History.Detail.Keypad",
        ["Mac"] = "History.Detail.Mac",
        ["Mask"] = "History.Detail.SubnetMask",
        ["MenuPass"] = "History.Detail.MenuPassword",
        ["MenuPassword"] = "History.Detail.MenuPassword",
        ["Message"] = "History.Detail.Message",
        ["MessageLine1"] = "History.Detail.DefaultMessage",
        ["MessageLine2"] = "History.Detail.SecondaryMessage",
        ["Name"] = "History.Detail.Name",
        ["Out"] = "History.Detail.Flow",
        ["Passage"] = "History.Detail.Passage",
        ["Payload"] = "History.Detail.Payload",
        ["PayloadSize"] = "History.Detail.PayloadSize",
        ["PictoWaitIn"] = "History.Detail.Flow",
        ["PictoWaitOut"] = "History.Detail.Flow",
        ["Port"] = "History.Detail.Port",
        ["RawData"] = "History.Detail.Payload",
        ["Ready"] = "History.Detail.Ready",
        ["Reason"] = "History.Detail.Message",
        ["ReleaseDuration"] = "History.Detail.ReleaseDuration",
        ["ReleaseTime"] = "History.Detail.ReleaseDuration",
        ["Result"] = "History.Detail.Result",
        ["ReturnCode"] = "History.Detail.ReturnCode",
        ["ReturnRaw"] = "History.Detail.Payload",
        ["Rfid"] = "History.Detail.Rfid",
        ["SerialNumber"] = "History.Detail.SerialNumber",
        ["ShowCounters"] = "History.Detail.ShowCounters",
        ["StaticIp"] = "History.Detail.IpMode",
        ["Status"] = "History.Detail.Status",
        ["SubnetMask"] = "History.Detail.SubnetMask",
        ["Success"] = "History.Detail.Result",
        ["Template"] = "History.Detail.Template",
        ["Timeout"] = "History.Detail.Timeout",
        ["TopRow"] = "History.Detail.DefaultMessage",
        ["Type"] = "History.Detail.Type",
        ["Summary"] = "History.Detail.Summary",
        ["Command"] = "History.Detail.Command"
    };

    public virtual CommandHistoryItemViewModel Format(
        DateTimeOffset timestamp,
        string commandId,
        CommandResultViewModel result)
    {
        var details = new List<HistoryDetailViewModel>();
        AddSemanticDetails(details, commandId, result);
        AddDetails(details, result.Data, null, 0);

        if (!string.IsNullOrWhiteSpace(result.TechnicalDetails) && details.Count < MaxDetails)
        {
            var sanitizedTechnicalDetails = SanitizeTechnicalDetails(result.TechnicalDetails);
            details.Add(string.IsNullOrWhiteSpace(sanitizedTechnicalDetails)
                ? new HistoryDetailViewModel("History.Detail.Message", "payload", "History.Value.PayloadSummaryUnavailable")
                : new HistoryDetailViewModel("History.Detail.Message", sanitizedTechnicalDetails));
        }

        return new CommandHistoryItemViewModel
        {
            Timestamp = timestamp,
            CommandId = commandId,
            Status = result.Status,
            MessageKey = result.MessageKey,
            DeviceName = result.Device?.Name,
            TechnicalDetails = result.TechnicalDetails,
            Details = details,
            Payload = CreatePayload(timestamp, commandId, result, details)
        };
    }

    protected static void AddDetails(
        List<HistoryDetailViewModel> details,
        object? value,
        string? prefix,
        int depth)
    {
        if (value is null || depth > MaxDepth || details.Count >= MaxDetails)
            return;

        if (value is JsonElement element)
        {
            AddJsonDetails(details, element, prefix, depth);
            return;
        }

        if (TryScalar(value, out var scalar, out var scalarKey))
        {
            details.Add(new HistoryDetailViewModel(ToLabelKey(prefix ?? "Value"), scalar, scalarKey));
            return;
        }

        if (value is byte[] bytes)
        {
            details.Add(new HistoryDetailViewModel("History.Detail.PayloadSize", bytes.Length.ToString()));
            return;
        }

        if (value is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (details.Count >= MaxDetails)
                    break;

                AddDetails(details, entry.Value, Join(prefix, entry.Key?.ToString()), depth + 1);
            }

            return;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            var count = 0;
            foreach (var item in enumerable)
            {
                if (details.Count >= MaxDetails || count >= 4)
                    break;

                AddDetails(details, item, Join(prefix, $"Item {count + 1}"), depth + 1);
                count++;
            }

            if (count == 0)
                details.Add(new HistoryDetailViewModel(ToLabelKey(prefix ?? "Items"), "0"));

            return;
        }

        foreach (var property in value.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0))
        {
            if (details.Count >= MaxDetails)
                break;

            object? propertyValue;
            try
            {
                propertyValue = property.GetValue(value);
            }
            catch
            {
                continue;
            }

            AddDetails(details, propertyValue, Join(prefix, property.Name), depth + 1);
        }
    }

    protected static HistoryMediaViewModel? TryCreateImageMedia(byte[] payload)
    {
        var mimeType = GetImageMimeType(payload);
        if (mimeType is null)
            return null;

        return new HistoryMediaViewModel(
            "image",
            $"data:{mimeType};base64,{Convert.ToBase64String(payload)}",
            "History.Media.BiometricsAlt",
            "History.Media.BiometricsCaption");
    }

    private static void AddSemanticDetails(
        List<HistoryDetailViewModel> details,
        string commandId,
        CommandResultViewModel result)
    {
        details.Add(new HistoryDetailViewModel(
            "History.Detail.Result",
            result.Status.ToString(),
            result.Status switch
            {
                CommandStatusKind.Pending => "History.Result.Pending",
                CommandStatusKind.Success => "History.Result.Success",
                CommandStatusKind.Warning => "History.Result.Warning",
                CommandStatusKind.Error => "History.Result.Error",
                CommandStatusKind.Canceled => "History.Result.Canceled",
                _ => "History.Result.Idle"
            }));

        if (CommandLabelKeys.TryGetValue(commandId, out var commandLabelKey))
        {
            details.Add(new HistoryDetailViewModel("History.Detail.Command", commandId, commandLabelKey));
        }
        else if (commandId.StartsWith("configuration.submit.", StringComparison.OrdinalIgnoreCase))
        {
            details.Add(new HistoryDetailViewModel("History.Detail.Command", commandId, "History.Event.ConfigurationSubmit"));
        }

        if (CommandEventKeys.TryGetValue(commandId, out var eventKey))
        {
            details.Add(new HistoryDetailViewModel("History.Detail.Summary", commandId, eventKey));
        }
        else if (commandId.StartsWith("configuration.submit.", StringComparison.OrdinalIgnoreCase))
        {
            details.Add(new HistoryDetailViewModel("History.Detail.Summary", commandId, "History.Event.ConfigurationSubmit"));
        }
    }

    protected static IReadOnlyDictionary<string, object?> CreatePayload(
        DateTimeOffset timestamp,
        string commandId,
        CommandResultViewModel result,
        IReadOnlyList<HistoryDetailViewModel> details,
        HistoryMediaViewModel? media = null)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["timestamp"] = timestamp,
            ["command"] = commandId,
            ["status"] = result.Status.ToString(),
            ["messageKey"] = result.MessageKey,
            ["device"] = CreateDevicePayload(result.Device),
            ["details"] = CreateDetailsPayload(details),
            ["technicalDetails"] = result.TechnicalDetails
        };

        if (media is not null && TryCreateJsonSafePayload(media, out var mediaPayload))
            payload["media"] = mediaPayload;

        if (TryCreateJsonSafePayload(result.Data, out var data))
            payload["data"] = data;
        else if (result.Data is not null)
            payload["data"] = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["summary"] = "History.Value.PayloadSummaryUnavailable",
                ["type"] = result.Data.GetType().FullName
            };

        return payload;
    }

    private static IReadOnlyDictionary<string, object?>? CreateDevicePayload(DeviceRefViewModel? device)
    {
        if (device is null)
            return null;

        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = device.Id,
            ["key"] = device.Key,
            ["name"] = device.Name,
            ["ipAddress"] = device.IpAddress,
            ["type"] = device.Type.ToString(),
            ["serialNumber"] = device.SerialNumber,
            ["port"] = device.Port,
            ["connected"] = device.Connected,
            ["modules"] = device.Modules.Select(module => module.ToString()).ToArray()
        };
    }

    private static IReadOnlyList<IReadOnlyDictionary<string, object?>> CreateDetailsPayload(
        IReadOnlyList<HistoryDetailViewModel> details)
    {
        var payload = new List<IReadOnlyDictionary<string, object?>>(details.Count);
        foreach (var detail in details)
        {
            payload.Add(new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["labelKey"] = detail.LabelKey,
                ["value"] = detail.Value,
                ["valueKey"] = detail.ValueKey
            });
        }

        return payload;
    }

    private static bool TryCreateJsonSafePayload(object? value, out object? payload)
    {
        payload = null;
        if (value is null)
            return true;

        try
        {
            payload = value is JsonElement element
                ? element.Clone()
                : JsonSerializer.SerializeToElement(value, value.GetType(), PayloadJsonOptions);
            return true;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    protected static string? GetImageMimeType(byte[] payload)
    {
        if (payload.Length >= 8 &&
            payload[0] == 0x89 &&
            payload[1] == 0x50 &&
            payload[2] == 0x4E &&
            payload[3] == 0x47 &&
            payload[4] == 0x0D &&
            payload[5] == 0x0A &&
            payload[6] == 0x1A &&
            payload[7] == 0x0A)
            return "image/png";

        if (payload.Length >= 3 &&
            payload[0] == 0xFF &&
            payload[1] == 0xD8 &&
            payload[2] == 0xFF)
            return "image/jpeg";

        if (payload.Length >= 2 &&
            payload[0] == 0x42 &&
            payload[1] == 0x4D)
            return "image/bmp";

        return null;
    }

    private static void AddJsonDetails(
        List<HistoryDetailViewModel> details,
        JsonElement element,
        string? prefix,
        int depth)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (details.Count >= MaxDetails)
                        break;

                    AddJsonDetails(details, property.Value, Join(prefix, property.Name), depth + 1);
                }
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    if (details.Count >= MaxDetails || index >= 4)
                        break;

                    AddJsonDetails(details, item, Join(prefix, $"Item {index + 1}"), depth + 1);
                    index++;
                }
                break;
            case JsonValueKind.String:
                details.Add(new HistoryDetailViewModel(ToLabelKey(prefix ?? "Value"), element.GetString() ?? string.Empty));
                break;
            case JsonValueKind.Number:
                details.Add(new HistoryDetailViewModel(ToLabelKey(prefix ?? "Value"), element.ToString()));
                break;
            case JsonValueKind.True:
                details.Add(new HistoryDetailViewModel(ToLabelKey(prefix ?? "Value"), "true", "History.Value.True"));
                break;
            case JsonValueKind.False:
                details.Add(new HistoryDetailViewModel(ToLabelKey(prefix ?? "Value"), "false", "History.Value.False"));
                break;
        }
    }

    private static string SanitizeTechnicalDetails(string technicalDetails)
    {
        var text = technicalDetails.Trim();
        if ((text.StartsWith('{') && text.EndsWith('}')) || (text.StartsWith('[') && text.EndsWith(']')))
            return string.Empty;

        return text.Length <= 180 ? text : $"{text[..180]}...";
    }

    private static bool TryScalar(object value, out string text, out string? valueKey)
    {
        valueKey = null;
        text = value switch
        {
            string stringValue => stringValue,
            bool boolValue => FormatBool(boolValue, out valueKey),
            byte or short or int or long or float or double or decimal => value.ToString() ?? string.Empty,
            Enum enumValue => FormatEnum(enumValue, out valueKey),
            DateTime dateTime => dateTime.ToString("G"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("G"),
            _ => string.Empty
        };

        return text.Length > 0;
    }

    private static string FormatBool(bool value, out string valueKey)
    {
        valueKey = value ? "History.Value.True" : "History.Value.False";
        return value ? "true" : "false";
    }

    private static string FormatEnum(Enum value, out string valueKey)
    {
        valueKey = $"History.Value.{value.GetType().Name}.{value}";
        return Humanize(value.ToString());
    }

    private static string Humanize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var chars = new List<char>(value.Length + 4) { value[0] };
        for (var index = 1; index < value.Length; index++)
        {
            var current = value[index];
            var previous = value[index - 1];
            if (char.IsUpper(current) && !char.IsWhiteSpace(previous) && !char.IsUpper(previous))
                chars.Add(' ');

            chars.Add(current);
        }

        return new string(chars.ToArray());
    }

    private static string Join(string? prefix, string? name) =>
        string.IsNullOrWhiteSpace(prefix) ? name ?? "Value" : $"{prefix}.{name}";

    private static string ToLabelKey(string label)
    {
        var segment = label.Split('.', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? label;
        return DetailLabelKeys.TryGetValue(segment, out var labelKey) ? labelKey : "History.Detail.Value";
    }
}
