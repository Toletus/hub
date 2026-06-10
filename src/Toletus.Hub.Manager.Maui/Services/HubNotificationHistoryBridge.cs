using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Models;
using Toletus.Hub.Manager.UI.Services;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices.Base;
using Toletus.LiteNet2.Command.Enums;
using Toletus.LiteNet3;
using Toletus.LiteNet3.Handler.Responses.Base;
using HubDeviceType = Toletus.Hub.Models.DeviceType;

namespace Toletus.Hub.Manager.Maui.Services;

public sealed class HubNotificationHistoryBridge : IDisposable
{
    private readonly NotificationHistoryService _history;
    private readonly ICommandHistoryFormatter _formatter;
    private readonly Dictionary<string, LiteNet3Board> _observedLiteNet3Boards = new(StringComparer.OrdinalIgnoreCase);

    public HubNotificationHistoryBridge(NotificationHistoryService history, ICommandHistoryFormatter formatter)
    {
        _history = history;
        _formatter = formatter;
        NotificationBaseService.OnNotification += HandleNotification;
        LiteNet3Devices.OnBoardReceived += ObserveLiteNet3Board;

        foreach (var board in LiteNet3Devices.Boards ?? [])
            ObserveLiteNet3Board(board);
    }

    private void HandleNotification(Notification notification)
    {
        _ = FormatAndAddNotificationAsync(notification);
    }

    private async Task FormatAndAddNotificationAsync(Notification notification)
    {
        if (!TryGetUiType(notification.Type, out var uiType))
            return;

        var device = new DeviceRefViewModel
        {
            Key = $"{uiType}:{notification.Ip}:{notification.Id}",
            Name = $"{uiType} {notification.Id}",
            IpAddress = notification.Ip,
            Type = uiType,
            Id = notification.Id,
            Connected = true
        };

        var isError = IsLiteNet3ErrorResponse(notification);
        var commandId = GetCommandId(notification);
        var messageKey = isError
            ? "Message.NotificationErrorReceived"
            : HasBiometricsPayload(notification.Response)
            ? "Message.BiometricsReceived"
            : "Message.NotificationReceived";
        var result = isError
            ? CommandResultViewModel.Error(messageKey, GetErrorSummary(notification.Response), device) with
            {
                Data = notification.Response
            }
            : CommandResultViewModel.Success(messageKey, device, notification.Response);

        var item = await Task.Run(() => _formatter.Format(DateTimeOffset.Now, commandId, result));

        MainThread.BeginInvokeOnMainThread(() =>
            _history.Add(item));
    }

    private void ObserveLiteNet3Board(LiteNet3Board board)
    {
        var key = board.Ip.ToString();
        if (_observedLiteNet3Boards.TryGetValue(key, out var existing))
        {
            if (ReferenceEquals(existing, board))
                return;

            existing.OnResponse -= Board_OnResponse;
        }

        _observedLiteNet3Boards[key] = board;
        board.OnResponse += Board_OnResponse;
    }

    private void UnobserveLiteNet3Boards()
    {
        foreach (var board in _observedLiteNet3Boards.Values)
        {
            board.OnResponse -= Board_OnResponse;
        }

        _observedLiteNet3Boards.Clear();
    }

    private void Board_OnResponse(LiteNet3BoardBase board, object response)
    {
        if (IsErrorResponse(response))
        {
            _ = AddLiteNet3ErrorResponseAsync(board, response);
            return;
        }

        if (IsPingResponse(response))
            _ = AddLiteNet3ResponseAsync(board, response, "notification.litenet3.ping", "Message.NotificationReceived");
    }

    private async Task AddLiteNet3ErrorResponseAsync(LiteNet3BoardBase board, object response)
    {
        await AddLiteNet3ResponseAsync(
            board,
            response,
            "notification.litenet3.error",
            "Message.NotificationErrorReceived",
            GetErrorSummary(response),
            isError: true);
    }

    private async Task AddLiteNet3ResponseAsync(
        LiteNet3BoardBase board,
        object response,
        string commandId,
        string messageKey,
        string? technicalDetails = null,
        bool isError = false)
    {
        var device = new DeviceRefViewModel
        {
            Key = $"{DeviceTypeKind.LiteNet3}:{board.Ip}:{board.Id}",
            Name = $"LiteNet3 {board.Id}",
            IpAddress = board.Ip.ToString(),
            Type = DeviceTypeKind.LiteNet3,
            Id = board.Id,
            Connected = true
        };

        var result = isError
            ? CommandResultViewModel.Error(messageKey, technicalDetails, device)
            : CommandResultViewModel.Success(messageKey, device, response);

        result = result with
        {
            Data = response
        };

        var item = await Task.Run(() => _formatter.Format(DateTimeOffset.Now, commandId, result));

        MainThread.BeginInvokeOnMainThread(() =>
            _history.Add(item));
    }

    private static bool HasBiometricsPayload(object? response)
    {
        if (response is null)
            return false;

        return response is byte[] ||
               response.GetType().GetProperty("Biometrics")?.GetValue(response) is byte[];
    }

    private static string GetCommandId(Notification notification)
    {
        var response = notification.Response;
        if (IsLiteNet3ErrorResponse(notification))
            return "notification.litenet3.error";

        if (HasBiometricsPayload(response))
            return "notification.litenet3.biometrics";

        if (notification.Type == HubDeviceType.SM25)
            return "notification.sm25";

        if (HasProperty(response, "Identification"))
            return GetIdentificationCommandId(notification, response);

        if (HasProperty(response, "Gyre") || HasProperty(response, "GyreResponse") || HasProperty(response, "Passage"))
            return "notification.passage";

        if (HasProperty(response, "Timeout"))
            return "notification.timeout";

        if (HasProperty(response, "Status"))
            return "notification.status";

        if (HasProperty(response, "IsReady"))
            return "notification.ready";

        if (HasProperty(response, "BoardConnectionStatus"))
            return "notification.connection";

        if (HasProperty(response, "FingerprintReaderConnected"))
            return "notification.fingerprint_reader";

        return $"notification.{notification.Type}.{notification.Command}";
    }

    private static bool HasProperty(object? value, string propertyName) =>
        value?.GetType().GetProperty(propertyName)?.GetValue(value) is not null;

    private static bool IsLiteNet3ErrorResponse(Notification notification)
    {
        if (notification.Type != HubDeviceType.LiteNet3)
            return false;

        if ((ResponseType)notification.Command == ResponseType.Error)
            return true;

        return IsErrorResponse(notification.Response) ||
               IsErrorResponse(notification.Response?.GetType().GetProperty("Data")?.GetValue(notification.Response));
    }

    private static bool IsErrorResponse(object? response) =>
        response?.GetType().Name.Contains("ErrorResponse", StringComparison.OrdinalIgnoreCase) == true;

    private static bool IsPingResponse(object? response) =>
        response?.GetType().Name.Contains("PingResponse", StringComparison.OrdinalIgnoreCase) == true;

    private static string? GetErrorSummary(object? response)
    {
        if (response is null)
            return "LiteNet3 error notification received.";

        foreach (var propertyName in new[] { "Message", "Error", "Reason", "Description", "Status", "Code" })
        {
            var value = response.GetType().GetProperty(propertyName)?.GetValue(response)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return response.GetType().Name;
    }

    private static string GetIdentificationCommandId(Notification notification, object? response)
    {
        if (notification.Type == HubDeviceType.LiteNet2)
        {
            return (LiteNet2Commands)notification.Command switch
            {
                LiteNet2Commands.IdentificationByRfId => "notification.identification.rfid",
                LiteNet2Commands.IdentificationByBarCode => "notification.identification.barcode",
                LiteNet2Commands.IdentificationByKeyboard => "notification.identification.keypad",
                LiteNet2Commands.PositiveIdentificationByFingerprintReader or
                    LiteNet2Commands.NegativeIdentificationByFingerprintReader => "notification.identification.fingerprint",
                _ => "notification.identification"
            };
        }

        if (notification.Type == HubDeviceType.LiteNet3)
        {
            var responseTypeName = Enum.GetName(typeof(ResponseType), notification.Command) ?? string.Empty;
            if (responseTypeName.Contains("Rfid", StringComparison.OrdinalIgnoreCase))
                return "notification.identification.rfid";

            if (responseTypeName.Contains("Barcode", StringComparison.OrdinalIgnoreCase))
                return "notification.identification.barcode";

            if (responseTypeName.Contains("Keypad", StringComparison.OrdinalIgnoreCase))
                return "notification.identification.keypad";
        }

        var identification = response?.GetType().GetProperty("Identification")?.GetValue(response);
        var typeName = identification?.GetType().Name ?? string.Empty;

        if (typeName.Contains("Rfid", StringComparison.OrdinalIgnoreCase))
            return "notification.identification.rfid";

        if (typeName.Contains("Barcode", StringComparison.OrdinalIgnoreCase))
            return "notification.identification.barcode";

        if (typeName.Contains("Keypad", StringComparison.OrdinalIgnoreCase))
            return "notification.identification.keypad";

        if (typeName.Contains("Fingerprint", StringComparison.OrdinalIgnoreCase))
            return "notification.identification.fingerprint";

        return "notification.identification";
    }

    private static bool TryGetUiType(HubDeviceType type, out DeviceTypeKind uiType)
    {
        uiType = type switch
        {
            HubDeviceType.LiteNet1 => DeviceTypeKind.LiteNet1,
            HubDeviceType.LiteNet2 => DeviceTypeKind.LiteNet2,
            HubDeviceType.LiteNet3 => DeviceTypeKind.LiteNet3,
            HubDeviceType.SM25 => DeviceTypeKind.SM25,
            _ => default
        };

        return type is HubDeviceType.LiteNet1 or HubDeviceType.LiteNet2 or HubDeviceType.LiteNet3 or HubDeviceType.SM25;
    }

    public void Dispose()
    {
        NotificationBaseService.OnNotification -= HandleNotification;
#pragma warning disable CS8601
        LiteNet3Devices.OnBoardReceived -= ObserveLiteNet3Board;
#pragma warning restore CS8601
        UnobserveLiteNet3Boards();
    }
}
