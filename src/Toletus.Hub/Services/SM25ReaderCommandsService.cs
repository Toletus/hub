using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices;
using Toletus.LiteNet2;
using Toletus.SM25;
using Toletus.SM25.Command.Enums;

namespace Toletus.Hub.Services;

// ReSharper disable once InconsistentNaming
public class SM25ReaderCommandsService : SM25NotificationService
{
    private static readonly Dictionary<string, SM25Reader> Readers = [];

    #region Reads Commands

    public async Task<Notification> GetDeviceName(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetDeviceName,
            () => reader?.Sync.GetDeviceName());
        Disconnect(device);

        return notification;
    }

    // ReSharper disable once InconsistentNaming
    public async Task<Notification> GetFWVersion(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetFWVersion,
            () => reader?.Sync.GetFWVersion());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetDeviceId(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetDeviceID,
            () => reader?.Sync.GetDeviceId());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetEmptyId(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetEmptyID,
            () => reader?.Sync.GetEmptyID());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetEnrollData(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetEnrollData,
            () => reader?.Sync.GetEnrollData());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetEnrollCount(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetEnrollCount,
            () => reader?.Sync.GetEnrollCount());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetTemplateStatus(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetTemplateStatus,
            () => reader?.Sync.GetTemplateStatus(id));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetDuplicationCheck(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetDuplicationCheck,
            () => reader?.Sync.GetDuplicationCheck());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetSecurityLevel(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetSecurityLevel,
            () => reader?.Sync.GetSecurityLevel());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetFingerTimeOut(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetFingerTimeOut,
            () => reader?.Sync.GetFingerTimeOut());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> ReadTemplate(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.ReadTemplate,
            () => reader?.Sync.ReadTemplate(id));

        return notification;
    }

    #endregion

    #region Writes Commands

    public async Task<Notification> Enroll(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.Enroll,
            () => reader?.Sync.Enroll(id));

        return notification;
    }

    // ReSharper disable once InconsistentNaming
    public async Task<Notification> EnrollAndStoreinRAM(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.EnrollAndStoreinRAM,
            () => reader?.Sync.EnrollAndStoreinRAM());

        return notification;
    }

    public async Task<Notification> ClearTemplate(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.ClearTemplate,
            () => reader?.Sync.ClearTemplate(id));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> ClearAllTemplate(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.ClearAllTemplate,
            () => reader?.Sync.ClearAllTemplate());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetDeviceId(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetDeviceID,
            () => reader?.Sync.SetDeviceId(id));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetFingerTimeOut(Device device, ushort timeOut)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetFingerTimeOut,
            () => reader?.Sync.SetFingerTimeOut(timeOut));
        Disconnect(device);

        return notification;
    }

    // ReSharper disable once InconsistentNaming
    public async Task<Notification> FPCancel(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.FPCancel,
            () => reader?.Sync.FPCancel());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetDuplicationCheck(Device device, bool check)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetDuplicationCheck,
            () => reader?.Sync.SetDuplicationCheck(check));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetSecurityLevel(Device device, ushort level)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetSecurityLevel,
            () => reader?.Sync.SetSecurityLevel(level));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> WriteTemplate(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.WriteTemplate,
            () => reader?.Sync.WriteTemplate());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> WriteTemplateData(Device device, ushort id, byte[] template)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.WriteTemplate,
            () => reader?.Sync.WriteTemplateData(id, template));
        Disconnect(device);

        return notification;
    }

    #endregion

    #region Privates Methods

    private static SM25Reader? GetReader(Device device)
    {
        var board = device.Get<LiteNet2Board>();

        return board is not { Connected: true } ? null : new SM25Reader(board.Ip);
    }

    private static SM25Reader? Connect(Device device)
    {
        Readers.TryGetValue(device.Ip, out var reader);

        if (reader is { Connected: true }) return reader;

        reader = GetReader(device);

        if (reader == null) return null;

        reader.Connect();
        Readers.TryAdd(device.Ip, reader);
        SubscribeSM25Reader(reader);
        return reader;
    }

    private static void Disconnect(Device device)
    {
        Readers.TryGetValue(device.Ip, out var reader);

        reader ??= GetReader(device);
        reader?.Close();
        Readers.Remove(device.Ip);
        UnsubscribeSM25Reader(reader);
    }

    #endregion
}