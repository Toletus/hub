namespace Toletus.Hub.Models;

public class DeviceResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object Data { get; set; }
    
    public DeviceResponse(object data = default, bool success = true)
    {
        Data = data;
        Success = success;
    }

    public DeviceResponse(bool success, string message, object? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
