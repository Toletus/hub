using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Services;

namespace Toletus.Hub.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("[controller]")]
public class WebhookController : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    [HttpPost(nameof(SetEndpoint))]
    public void SetEndpoint(string endpoint)
    {
        WebhookSerivce.SetEndpoint(endpoint);
    }
}