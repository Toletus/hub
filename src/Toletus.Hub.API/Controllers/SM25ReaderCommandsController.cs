using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Services;

namespace Toletus.Hub.Controllers;

[ApiController]
[Route("[controller]")]
// ReSharper disable once InconsistentNaming
public class SM25ReaderCommandsController(SM25ReaderCommandsService readerCommandsService) : BaseController
{
    #region Reads Commands

    [HttpPost(nameof(GetDeviceName))]
    public async Task<IActionResult> GetDeviceName(Device device)
    {
        return GetResponse(await readerCommandsService.GetDeviceName(device));
    }

    [HttpPost(nameof(GetFWVersion))]
    public async Task<IActionResult> GetFWVersion(Device device)
    {
        return GetResponse(await readerCommandsService.GetFWVersion(device));
    }

    [HttpPost(nameof(GetDeviceId))]
    public async Task<IActionResult> GetDeviceId(Device device)
    {
        return GetResponse(await readerCommandsService.GetDeviceId(device));
    }

    [HttpPost(nameof(GetEmptyID))]
    public async Task<IActionResult> GetEmptyID(Device device)
    {
        return GetResponse(await readerCommandsService.GetEmptyId(device));
    }

    [HttpPost(nameof(GetEnrollCount))]
    public async Task<IActionResult> GetEnrollCount(Device device)
    {
        return GetResponse(await readerCommandsService.GetEnrollCount(device));
    }

    [HttpPost(nameof(GetEnrollData))]
    public async Task<IActionResult> GetEnrollData(Device device)
    {
        return GetResponse(await readerCommandsService.GetEnrollData(device));
    }

    [HttpPost(nameof(GetTemplateStatus))]
    public async Task<IActionResult> GetTemplateStatus(Device device, ushort id)
    {
        return GetResponse(await readerCommandsService.GetTemplateStatus(device, id));
    }

    [HttpPost(nameof(GetDuplicationCheck))]
    public async Task<IActionResult> GetDuplicationCheck(Device device)
    {
        return GetResponse(await readerCommandsService.GetDuplicationCheck(device));
    }

    [HttpPost(nameof(GetSecurityLevel))]
    public async Task<IActionResult> GetSecurityLevel(Device device)
    {
        return GetResponse(await readerCommandsService.GetSecurityLevel(device));
    }

    [HttpPost(nameof(GetFingerTimeOut))]
    public async Task<IActionResult> GetFingerTimeOut(Device device)
    {
        return GetResponse(await readerCommandsService.GetFingerTimeOut(device));
    }

    [HttpPost(nameof(ReadTemplate))]
    public async Task<IActionResult> ReadTemplate(Device device, ushort id)
    {
        return GetResponse(await readerCommandsService.ReadTemplate(device, id));
    }

    #endregion

    #region Writes Commands

    [HttpPost(nameof(Enroll))]
    public async Task<IActionResult> Enroll(Device device, ushort id)
    {
        return GetResponse(await readerCommandsService.Enroll(device, id));
    }

    [HttpPost(nameof(EnrollAndStoreinRAM))]
    public async Task<IActionResult> EnrollAndStoreinRAM(Device device)
    {
        return GetResponse(await readerCommandsService.EnrollAndStoreinRAM(device));
    }

    [HttpPost(nameof(ClearTemplate))]
    public async Task<IActionResult> ClearTemplate(Device device, ushort id)
    {
        return GetResponse(await readerCommandsService.ClearTemplate(device, id));
    }

    [HttpPost(nameof(ClearAllTemplate))]
    public async Task<IActionResult> ClearAllTemplate(Device device)
    {
        return GetResponse(await readerCommandsService.ClearAllTemplate(device));
    }

    [HttpPost(nameof(SetDeviceId))]
    public async Task<IActionResult> SetDeviceId(Device device, ushort id)
    {
        return GetResponse(await readerCommandsService.SetDeviceId(device, id));
    }

    [HttpPost(nameof(SetFingerTimeOut))]
    public async Task<IActionResult> SetFingerTimeOut(Device device, ushort timeOut)
    {
        return GetResponse(await readerCommandsService.SetFingerTimeOut(device, timeOut));
    }

    [HttpPost(nameof(FPCancel))]
    public async Task<IActionResult> FPCancel(Device device)
    {
        return GetResponse(await readerCommandsService.FPCancel(device));
    }

    [HttpPost(nameof(SetDuplicationCheck))]
    public async Task<IActionResult> SetDuplicationCheck(Device device, bool check)
    {
        return GetResponse(await readerCommandsService.SetDuplicationCheck(device, check));
    }

    [HttpPost(nameof(SetSecurityLevel))]
    public async Task<IActionResult> SetSecurityLevel(Device device, ushort level)
    {
        return GetResponse(await readerCommandsService.SetSecurityLevel(device, level));
    }

    [HttpPost(nameof(WriteTemplate))]
    public async Task<IActionResult> WriteTemplate(Device device)
    {
        return GetResponse(await readerCommandsService.WriteTemplate(device));
    }

    [HttpPost(nameof(WriteTemplateData))]
    public async Task<IActionResult> WriteTemplateData(WriteTemplateDataRequest request)
    {
        return GetResponse(await readerCommandsService.WriteTemplateData(request.Device, request.Id, request.Template));
    }

    #endregion
}