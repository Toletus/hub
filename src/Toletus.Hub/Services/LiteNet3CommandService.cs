using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices;
using Toletus.LiteNet3;
using Toletus.LiteNet3.Handler.Responses.Base;

namespace Toletus.Hub.Services;

public class LiteNet3CommandService : LiteNet3NotificationService
{
    #region Reads Commands

    public async Task<Notification> GetBuzzerMute(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Buzzer,
            () => device.Get<LiteNet3Board>()?.GetBuzzerMute());
    }

    public async Task<Notification> GetFlow(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Flow,
            () => device.Get<LiteNet3Board>()?.GetFlow());
    }

    public async Task<Notification> GetStatusAndConfigurations(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.GetStatusAndConfigurations());
    }

    public async Task<Notification> GetEthernet(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Ethernet,
            () => device.Get<LiteNet3Board>()?.GetEthernet());
    }

    public async Task<Notification> GetDisplay(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Display,
            () => device.Get<LiteNet3Board>()?.GetDisplay());
    }

    public async Task<Notification> GetSensor(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Sensor,
            () => device.Get<LiteNet3Board>()?.GetSensor());
    }

    public async Task<Notification> GetFactory(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Factory,
            () => device.Get<LiteNet3Board>()?.GetFactory());
    }

    #endregion

    #region Writes Commands

    public async Task<Notification> Notify(Device device, string cmd, int time, string alignBot, string topRow,
        string bottomRow)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Display,
            () => device.Get<LiteNet3Board>()?.Notify(cmd, time, alignBot, topRow, bottomRow));
    }

    public async Task<Notification> ReleaseEntry(Device device, string? topRow = null, string? bottomRow = null)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.ReleaseEntry(topRow, bottomRow));
    }

    public async Task<Notification> ReleaseEntryAndExit(Device device, string? topRow = null, string? bottomRow = null)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.ReleaseEntryAndExit(topRow, bottomRow));
    }

    public async Task<Notification> ReleaseExit(Device device, string? topRow = null, string? bottomRow = null)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.ReleaseExit(topRow, bottomRow));
    }

    public async Task<Notification> Reset(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.Reset(),
            waitResponse: false);
    }

    public async Task<Notification> ResetSensor(Device device, bool resetIn, bool resetOut)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Sensor,
            () => device.Get<LiteNet3Board>()?.ResetSensor(resetIn, resetOut));
    }

    public async Task<Notification> SetId(Device device, int id)
    {
        device.Id = id;
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.SetId(id));
    }

    public async Task<Notification> SetAlias(Device device, string alias)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.SetAlias(alias));
    }

    public async Task<Notification> SetBuzzerMute(Device device, bool mute)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Buzzer,
            () => device.Get<LiteNet3Board>()?.SetBuzzerMute(mute));
    }

    public async Task<Notification> BuzzerPlay(Device device, string play)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Buzzer,
            () => device.Get<LiteNet3Board>()?.BuzzerPlay(play));
    }

    public async Task<Notification> BuzzerStop(Device device)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Buzzer,
            () => device.Get<LiteNet3Board>()?.BuzzerStop());
    }

    public async Task<Notification> BuzzerMute(Device device, bool mute)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Buzzer,
            () => device.Get<LiteNet3Board>()?.BuzzerMute(mute));
    }

    public async Task<Notification> SetFlow(
        Device device, bool inverted, string @in, string @out, string frontWait, int pictoWaitIn, int pictoWaitOut)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Flow,
            () => device.Get<LiteNet3Board>()?.SetFlow(inverted, @in, @out, frontWait, pictoWaitIn, pictoWaitOut));
    }

    public async Task<Notification> SetIpConfigurartion(Device device, string ip, string mask, string gateway)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Ethernet,
            () => device.Get<LiteNet3Board>()?.SetIpConfigurartion(ip, mask, gateway));
    }

    public async Task<Notification> SetStaticIp(Device device, bool staticIp)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Ethernet,
            () => device.Get<LiteNet3Board>()?.SetStaticIp(staticIp));
    }

    public async Task<Notification> SetMac(Device device, string macAddress)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Ethernet,
            () => device.Get<LiteNet3Board>()?.SetMacAddress(macAddress));
    }

    public async Task<Notification> SetMenuPassword(Device device, string password)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.SetMenuPassword(password));
    }

    public async Task<Notification> SetDisplay(Device device, string? topRow, string? bottomRow, string? mode)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.Display,
            () => device.Get<LiteNet3Board>()?.SetDisplay(topRow, bottomRow, mode));
    }

    public async Task<Notification> SetReleaseDuration(Device device, int releaseDuration)
    {
        return await ExecuteCommandAsync(
            device,
            ResponseType.LiteNet3,
            () => device.Get<LiteNet3Board>()?.SetReleaseDuration(releaseDuration));
    }

    #endregion
}