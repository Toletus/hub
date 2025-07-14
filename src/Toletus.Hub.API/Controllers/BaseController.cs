using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;

namespace Toletus.Hub.Controllers;

[ApiController]
[Route("[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult GetResponse(Notification notification)
    {
        if (notification.Response is DeviceResponse { Success: false })
            return BadRequest(notification);

        return Ok(notification);
    }
    
    protected IActionResult GetResponse(DeviceResponse response)
    {
        if (response.Success is false)
            return BadRequest(response);

        return Ok(response);
    }
}