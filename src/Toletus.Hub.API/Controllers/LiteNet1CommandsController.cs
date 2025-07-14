using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Services;
using Toletus.LiteNet1.Enums;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Toletus.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class LiteNet1CommandsController(LiteNet1CommandService liteNet1CommandService) : BaseController
{
    #region Reads Commands

    [HttpPost(nameof(GetCounters))]
    public async Task<IActionResult> GetCounters(Device device)
    {
        return GetResponse(await liteNet1CommandService.GetCounters(device));
    }

    [HttpPost(nameof(GetFirmwareVersion))]
    public async Task<IActionResult> GetFirmwareVersion(Device device)
    {
        return GetResponse(await liteNet1CommandService.GetFirmwareVersion(device));
    }

    [HttpPost(nameof(GetId))]
    public async Task<IActionResult> GetId(Device device)
    {
        return GetResponse(await liteNet1CommandService.GetId(device));
    }

    [HttpPost(nameof(GetIpMode))]
    public async Task<IActionResult> GetIpMode(Device device)
    {
        return GetResponse(await liteNet1CommandService.GetIpMode(device));
    }

    [HttpPost(nameof(GetShowCounters))]
    public async Task<IActionResult> GetShowCounters(Device device)
    {
        return GetResponse(await liteNet1CommandService.GetShowCounters(device));
    }

    [HttpPost(nameof(GetAll))]
    public async Task<IActionResult> GetAll(Device device)
    {
        return GetResponse(await liteNet1CommandService.GetAll(device));
    }

    #endregion

    #region Writes Commands

    [HttpPost(nameof(ReleaseEntry))]
    public async Task<IActionResult> ReleaseEntry(Device device, string message)
    {
        return GetResponse(await liteNet1CommandService.ReleaseEntry(device, message));
    }

    [HttpPost(nameof(ReleaseExit))]
    public async Task<IActionResult> ReleaseExit(Device device, string message)
    {
        return GetResponse(await liteNet1CommandService.ReleaseExit(device, message));
    }

    [HttpPost(nameof(Reset))]
    public async Task<IActionResult> Reset(Device device)
    {
        return GetResponse(await liteNet1CommandService.Reset(device));
    }

    [HttpPost(nameof(ResetCounters))]
    public async Task<IActionResult> ResetCounters(Device device)
    {
        return GetResponse(await liteNet1CommandService.ResetCounters(device));
    }

    [HttpPost(nameof(SetId))]
    public async Task<IActionResult> SetId(Device device, int id)
    {
        return GetResponse(await liteNet1CommandService.SetId(device, id));
    }

    [HttpPost(nameof(SetIp))]
    public async Task<IActionResult> SetIp(Device device, string ip)
    {
        return GetResponse(await liteNet1CommandService.SetIp(device, ip));
    }

    [HttpPost(nameof(SetBuzzerMute))]
    public async Task<IActionResult> SetBuzzerMute(Device device, bool on)
    {
        return GetResponse(await liteNet1CommandService.SetBuzzerMute(device, on));
    }

    [HttpPost(nameof(SetEntryClockwise))]
    public async Task<IActionResult> SetEntryClockwise(Device device, bool entryClockwise)
    {
        return GetResponse(await liteNet1CommandService.SetEntryClockwise(device, entryClockwise));
    }

    [HttpPost(nameof(SetFlowControl))]
    public async Task<IActionResult> SetFlowControl(Device device, ModoFluxo controlledFlow)
    {
        return GetResponse(await liteNet1CommandService.SetFlowControl(device, controlledFlow));
    }

    [HttpPost(nameof(SetShowCounters))]
    public async Task<IActionResult> SetShowCounters(Device device, bool showCounters)
    {
        return GetResponse(await liteNet1CommandService.SetShowCounters(device, showCounters));
    }

    [HttpPost(nameof(SetAll))]
    public async Task<IActionResult> SetAll(
        Device device,
        ModoFluxo controleFluxo,
        ModoEntrada modoIdentificacao,
        bool mudo,
        int id,
        bool entradaSentidoHorario,
        bool exibirRelogio,
        bool exibirContador,
        byte duracaoAcionamento,
        string mensagemPadrao,
        string mensagemSecundaria)
    {
        return GetResponse(await liteNet1CommandService.SetAll(
            device,
            controleFluxo,
            modoIdentificacao,
            mudo,
            id,
            entradaSentidoHorario,
            exibirRelogio,
            exibirContador,
            duracaoAcionamento,
            mensagemPadrao,
            mensagemSecundaria));
    }

    #endregion
}