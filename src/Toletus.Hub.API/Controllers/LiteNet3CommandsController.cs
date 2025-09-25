using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Toletus.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class LiteNet3CommandsController(LiteNet3CommandService liteNet3CommandService) : BaseController
{
    #region Reads Commands

    [HttpPost(nameof(GetBuzzerMute))]
    public async Task<IActionResult> GetBuzzerMute(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetBuzzerMute(device));
    }

    [HttpPost(nameof(GetFlow))]
    public async Task<IActionResult> GetFlow(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetFlow(device));
    }

    [HttpPost(nameof(GetStatusAndConfigurations))]
    public async Task<IActionResult> GetStatusAndConfigurations(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetStatusAndConfigurations(device));
    }

    [HttpPost(nameof(GetEthernet))]
    public async Task<IActionResult> GetEthernet(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetEthernet(device));
    }

    [HttpPost(nameof(GetDisplay))]
    public async Task<IActionResult> GetDisplay(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetDisplay(device));
    }

    [HttpPost(nameof(GetSensor))]
    public async Task<IActionResult> GetSensor(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetSensor(device));
    }

    [HttpPost(nameof(GetFactory))]
    public async Task<IActionResult> GetFactory(Device device)
    {
        return GetResponse(await liteNet3CommandService.GetFactory(device));
    }

    #endregion

    #region Writes Commands
    
    [HttpPost(nameof(Notify))]
    public async Task<IActionResult> Notify(Device device, string cmd, int time, string alignBot, string topRow,
        string bottomRow)
    {
        return GetResponse(await liteNet3CommandService.Notify(device, cmd, time, alignBot, topRow, bottomRow));
    }
    
    [HttpPost(nameof(ReleaseEntry))]
    public async Task<IActionResult> ReleaseEntry(Device device, string? topRow, string? bottomRow)
    {
        return GetResponse(await liteNet3CommandService.ReleaseEntry(device, topRow, bottomRow));
    }

    [HttpPost(nameof(ReleaseEntryAndExit))]
    public async Task<IActionResult> ReleaseEntryAndExit(Device device, string? topRow, string? bottomRow)
    {
        return GetResponse(await liteNet3CommandService.ReleaseEntryAndExit(device, topRow, bottomRow));
    }

    [HttpPost(nameof(ReleaseExit))]
    public async Task<IActionResult> ReleaseExit(Device device, string? topRow, string? bottomRow)
    {
        return GetResponse(await liteNet3CommandService.ReleaseExit(device, topRow, bottomRow));
    }

    [HttpPost(nameof(Reset))]
    public async Task<IActionResult> Reset(Device device)
    {
        return GetResponse(await liteNet3CommandService.Reset(device));
    }

    [HttpPost(nameof(ResetCounters))]
    public async Task<IActionResult> ResetCounters(Device device, bool resetIn, bool resetOut)
    {
        return GetResponse(await liteNet3CommandService.ResetSensor(device, resetIn, resetOut));
    }

    [HttpPost(nameof(SetId))]
    public async Task<IActionResult> SetId(Device device, int id)
    {
        return GetResponse(await liteNet3CommandService.SetId(device, id));
    }

    [HttpPost(nameof(SetAlias))]
    public async Task<IActionResult> SetAlias(Device device, string alias)
    {
        return GetResponse(await liteNet3CommandService.SetAlias(device, alias));
    }

    [HttpPost(nameof(SetBuzzerMute))]
    public async Task<IActionResult> SetBuzzerMute(Device device, bool mute)
    {
        return GetResponse(await liteNet3CommandService.SetBuzzerMute(device, mute));
    }

    [HttpPost(nameof(BuzzerPlay))]
    public async Task<IActionResult> BuzzerPlay(Device device, string play)
    {
        return GetResponse(await liteNet3CommandService.BuzzerPlay(device, play));
    }

    [HttpPost(nameof(BuzzerStop))]
    public async Task<IActionResult> BuzzerStop(Device device)
    {
        return GetResponse(await liteNet3CommandService.BuzzerStop(device));
    }

    [HttpPost(nameof(BuzzerMute))]
    public async Task<IActionResult> BuzzerMute(Device device, bool mute)
    {
        return GetResponse(await liteNet3CommandService.BuzzerMute(device, mute));
    }

    [HttpPost(nameof(SetFlow))]
    public async Task<IActionResult> SetFlow(
        Device device, bool inverted, string @in, string @out, string frontWait, int pictoWaitIn, int pictoWaitOut)
    {
        return GetResponse(await liteNet3CommandService.SetFlow(
            device, inverted, @in, @out, frontWait, pictoWaitIn, pictoWaitOut));
    }

    [HttpPost(nameof(SetIpConfigurartion))]
    public async Task<IActionResult> SetIpConfigurartion(Device device, string ip, string mask, string gateway)
    {
        return GetResponse(await liteNet3CommandService.SetIpConfigurartion(device, ip, mask, gateway));
    }

    [HttpPost(nameof(SetStaticIp))]
    public async Task<IActionResult> SetStaticIp(Device device, bool staticIp)
    {
        return GetResponse(await liteNet3CommandService.SetStaticIp(device, staticIp));
    }

    [HttpPost(nameof(SetMac))]
    public async Task<IActionResult> SetMac(Device device, string macAddress)
    {
        return GetResponse(await liteNet3CommandService.SetMac(device, macAddress));
    }

    [HttpPost(nameof(SetMenuPassword))]
    public async Task<IActionResult> SetMenuPassword(Device device, string password)
    {
        return GetResponse(await liteNet3CommandService.SetMenuPassword(device, password));
    }

    [HttpPost(nameof(SetDisplay))]
    public async Task<IActionResult> SetDisplay(Device device, string? topRow, string? bottomRow, string? mode)
    {
        return GetResponse(await liteNet3CommandService.SetDisplay(device, topRow, bottomRow, mode));
    }

    [HttpPost(nameof(SetReleaseDuration))]
    public async Task<IActionResult> SetReleaseDuration(Device device, int releaseDuration)
    {
        return GetResponse(await liteNet3CommandService.SetReleaseDuration(device, releaseDuration));
    }

    #endregion
}