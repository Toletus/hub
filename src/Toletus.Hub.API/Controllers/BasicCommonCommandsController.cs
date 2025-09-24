using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Services;

namespace Toletus.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public class BasicCommonCommandsController(BasicCommonCommandService basicCommonCommandService) : ControllerBase
{
    [HttpPost(nameof(GetId))]
    public IActionResult GetId(Device device)
    {
        return Ok(basicCommonCommandService.GetId(device));
    }

    [HttpPost(nameof(ReleaseEntry))]
    public IActionResult ReleaseEntry(Device device, string message)
    {
        return Ok(basicCommonCommandService.ReleaseEntry(device, message));
    }

    [HttpPost(nameof(ReleaseEntryAndExit))]
    public IActionResult ReleaseEntryAndExit(Device device, string message)
    {
        return Ok(basicCommonCommandService.ReleaseEntryAndExit(device, message));
    }

    [HttpPost(nameof(ReleaseExit))]
    public IActionResult ReleaseExit(Device device, string message)
    {
        return Ok(basicCommonCommandService.ReleaseExit(device, message));
    }

    [HttpPost(nameof(SetId))]
    public IActionResult SetId(Device device)
    {
        return Ok(basicCommonCommandService.SetId(device));
    }

    [HttpPost(nameof(SetEntryClockwise))]
    public IActionResult SetEntryClockwise(Device device)
    {
        return Ok(basicCommonCommandService.SetEntryClockwise(device));
    }

    [HttpPost(nameof(SetFlowControl))]
    public IActionResult SetFlowControl(Device device)
    {
        return Ok(basicCommonCommandService.SetFlowControl(device));
    }
}
