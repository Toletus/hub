using RestSharp;
using Toletus.Hub.Notifications;

namespace Toletus.Hub.Services;

public static class WebhookSerivce
{
    private static string? _endpoint;
    public static void SetEndpoint(string endpoint)
    {
        _endpoint = endpoint;
    }

    public static async Task PostDeviceResponse(Notification notification)
    {
        if (string.IsNullOrWhiteSpace(_endpoint)) return;
        
        var options = new RestClientOptions(_endpoint);
        var client = new RestClient(options);
        var request = new RestRequest("", Method.Post);
        request.AddHeader("accept", "*/*");
        request.AddHeader("Content-Type", "application/json");
        request.AddBody(notification);
        await client.ExecuteAsync(request);
    }
}