using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices;
using Toletus.LiteNet1;
using Toletus.LiteNet1.Enums;

namespace Toletus.Hub.Services;

public class LiteNet1CommandService : LiteNet1NotificationService
{
    #region Reads Commands

    public async Task<Notification> GetCounters(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.GetContadores(true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> GetFirmwareVersion(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.GetVersao()
                : null,
            parameters: new { });
    }

    public async Task<Notification> GetId(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.GetId(true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> GetIpMode(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.GetModoIp(true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> GetShowCounters(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.GetContadores(true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> GetAll(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.GetGeral()
                : null,
            parameters: new { });
    }

    #endregion

    #region Writes Commands

    public async Task<Notification> ReleaseEntry(Device device, string message)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.LiberarEntrada(message)
                : null,
            parameters: new { });
    }

    public async Task<Notification> ReleaseExit(Device device, string message)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.LiberarSaida(message)
                : null,
            parameters: new { });
    }

    public async Task<Notification> Reset(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.Resetar(true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> ResetCounters(Device device)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.ZerarContador(true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetId(Device device, int id)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetId(id)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetIp(Device device, string ip)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetModoIp(ip, true)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetBuzzerMute(Device device, bool on)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetMudo(on)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetEntryClockwise(Device device, bool entryClockwise)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetarSentidoEntrada(entryClockwise)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetFlowControl(Device device, ModoFluxo controlledFlow)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetarControleFluxo(controlledFlow)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetShowCounters(Device device, bool showCounters)
    {
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetContador(showCounters)
                : null,
            parameters: new { });
    }

    public async Task<Notification> SetAll(
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
        return await ExecuteCommandWithResultAsync(
            device,
            device.Get<LiteNet>() is { } board
                ? _ => board.SetGeral(
                    controleFluxo,
                    modoIdentificacao,
                    mudo,
                    id,
                    entradaSentidoHorario,
                    exibirRelogio,
                    exibirContador,
                    duracaoAcionamento,
                    mensagemPadrao,
                    mensagemSecundaria,
                    true)
                : null,
            parameters: new { });
    }

    #endregion
}