using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Services;
using Toletus.LiteNet2.Command.Enums;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Toletus.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class LiteNet2CommandsController(LiteNet2CommandService liteNet2CommandService) : BaseController
{
    #region Reads Commands

    [HttpPost(nameof(GetBuzzerMute))]
    public async Task<IActionResult> GetBuzzerMute(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetBuzzerMute(device));
    }

    [HttpPost(nameof(GetCounters))]
    public async Task<IActionResult> GetCounters(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetCounters(device));
    }

    [HttpPost(nameof(GetEntryClockwise))]
    public async Task<IActionResult> GetEntryClockwise(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetEntryClockwise(device));
    }

    [HttpPost(nameof(GetFingerprintIdentificationMode))]
    public async Task<IActionResult> GetFingerprintIdentificationMode(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetFingerprintIdentificationMode(device));
    }

    [HttpPost(nameof(GetFirmwareVersion))]
    public async Task<IActionResult> GetFirmwareVersion(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetFirmwareVersion(device));
    }

    [HttpPost(nameof(GetFlowControl))]
    public async Task<IActionResult> GetFlowControl(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetFlowControl(device));
    }

    [HttpPost(nameof(GetFlowControlExtended))]
    public async Task<IActionResult> GetFlowControlExtended(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetFlowControlExtended(device));
    }

    [HttpPost(nameof(GetId))]
    public async Task<IActionResult> GetId(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetId(device));
    }

    [HttpPost(nameof(GetIpMode))]
    public async Task<IActionResult> GetIpMode(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetIpMode(device));
    }

    [HttpPost(nameof(GetMac))]
    public async Task<IActionResult> GetMac(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetMac(device));
    }

    [HttpPost(nameof(GetMenuPassword))]
    public async Task<IActionResult> GetMenuPassword(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetMenuPassword(device));
    }

    [HttpPost(nameof(GetMessageLine1))]
    public async Task<IActionResult> GetMessageLine1(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetMessageLine1(device));
    }

    [HttpPost(nameof(GetMessageLine2))]
    public async Task<IActionResult> GetMessageLine2(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetMessageLine2(device));
    }

    [HttpPost(nameof(GetReleaseDuration))]
    public async Task<IActionResult> GetReleaseDuration(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetReleaseDuration(device));
    }

    [HttpPost(nameof(GetSerialNumber))]
    public async Task<IActionResult> GetSerialNumber(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetSerialNumber(device));
    }

    [HttpPost(nameof(GetShowCounters))]
    public async Task<IActionResult> GetShowCounters(Device device)
    {
        return GetResponse(await liteNet2CommandService.GetShowCounters(device));
    }

    #endregion

    #region Writes Commands

    [HttpPost(nameof(Notify))]
    public async Task<IActionResult> Notify(Device device, int duration = 1000, int tone = 0, int color = 0,
        int showMessage = 0)
    {
        return GetResponse(await liteNet2CommandService.Notify(device, duration, tone, color, showMessage));
    }

    [HttpPost(nameof(ReleaseEntry))]
    public async Task<IActionResult> ReleaseEntry(Device device, string message)
    {
        return GetResponse(await liteNet2CommandService.ReleaseEntry(device, message));
    }

    [HttpPost(nameof(ReleaseEntryAndExit))]
    public async Task<IActionResult> ReleaseEntryAndExit(Device device, string message)
    {
        return GetResponse(await liteNet2CommandService.ReleaseEntryAndExit(device, message));
    }

    [HttpPost(nameof(ReleaseExit))]
    public async Task<IActionResult> ReleaseExit(Device device, string message)
    {
        return GetResponse(await liteNet2CommandService.ReleaseExit(device, message));
    }

    [HttpPost(nameof(Reset))]
    public async Task<IActionResult> Reset(Device device)
    {
        return GetResponse(await liteNet2CommandService.Reset(device));
    }

    [HttpPost(nameof(ResetCounters))]
    public async Task<IActionResult> ResetCounters(Device device)
    {
        return GetResponse(await liteNet2CommandService.ResetCounters(device));
    }

    [HttpPost(nameof(ResetPeripherals))]
    public async Task<IActionResult> ResetPeripherals(Device device)
    {
        return GetResponse(await liteNet2CommandService.ResetPeripherals(device));
    }

    [HttpPost(nameof(SetId))]
    public async Task<IActionResult> SetId(Device device)
    {
        return GetResponse(await liteNet2CommandService.SetId(device));
    }

    [HttpPost(nameof(SetIp))]
    public async Task<IActionResult> SetIp(Device device, bool dhcp, string ip, string subnetMask)
    {
        return GetResponse(await liteNet2CommandService.SetIp(device, dhcp, ip, subnetMask));
    }

    [HttpPost(nameof(SetBuzzerMute))]
    public async Task<IActionResult> SetBuzzerMute(Device device, bool on)
    {
        return GetResponse(await liteNet2CommandService.SetBuzzerMute(device, on));
    }

    [HttpPost(nameof(SetEntryClockwise))]
    public async Task<IActionResult> SetEntryClockwise(Device device, bool entryClockwise)
    {
        return GetResponse(await liteNet2CommandService.SetEntryClockwise(device, entryClockwise));
    }

    [HttpPost(nameof(SetFlowControl))]
    public async Task<IActionResult> SetFlowControl(Device device, ControlledFlow controlledFlow)
    {
        return GetResponse(await liteNet2CommandService.SetFlowControl(device, controlledFlow));
    }

    [HttpPost(nameof(SetFingerprintIdentificationMode))]
    public async Task<IActionResult> SetFingerprintIdentificationMode(Device device, FingerprintIdentificationMode mode)
    {
        return GetResponse(await liteNet2CommandService.SetFingerprintIdentificationMode(device, mode));
    }

    [HttpPost(nameof(SetFlowControlExtended))]
    public async Task<IActionResult> SetFlowControlExtended(Device device, ControlledFlowExtended flowControlExtended)
    {
        return GetResponse(await liteNet2CommandService.SetFlowControlExtended(device, flowControlExtended));
    }

    [HttpPost(nameof(SetMac))]
    public async Task<IActionResult> SetMac(Device device, string mac)
    {
        return GetResponse(await liteNet2CommandService.SetMac(device, mac));
    }

    [HttpPost(nameof(SetMenuPassword))]
    public async Task<IActionResult> SetMenuPassword(Device device, string password)
    {
        return GetResponse(await liteNet2CommandService.SetMenuPassword(device, password));
    }

    [HttpPost(nameof(SetMessageLine1))]
    public async Task<IActionResult> SetMessageLine1(Device device, string message)
    {
        return GetResponse(await liteNet2CommandService.SetMessageLine1(device, message));
    }

    [HttpPost(nameof(SetMessageLine2))]
    public async Task<IActionResult> SetMessageLine2(Device device, string message)
    {
        return GetResponse(await liteNet2CommandService.SetMessageLine2(device, message));
    }

    [HttpPost(nameof(SetReleaseDuration))]
    public async Task<IActionResult> SetReleaseDuration(Device device, int releaseDuration)
    {
        return GetResponse(await liteNet2CommandService.SetReleaseDuration(device, releaseDuration));
    }

    [HttpPost(nameof(SetShowCounters))]
    public async Task<IActionResult> SetShowCounters(Device device, bool showCounters)
    {
        return GetResponse(await liteNet2CommandService.SetShowCounters(device, showCounters));
    }

    [HttpPost(nameof(TempMessage))]
    public async Task<IActionResult> TempMessage(Device device, string message)
    {
        return GetResponse(await liteNet2CommandService.TempMessage(device, message));
    }

    #endregion
}