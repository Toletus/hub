using Toletus.Hub.Models;

namespace Toletus.Hub.Services;

public class BasicCommonCommandService
{
    public DeviceResponse ReleaseEntry(Device device, string message)
    {
        device.ReleaseEntry(message);
        var data = new { Action = "Entry released" }; // Substitua com a lógica real
        return new DeviceResponse(true, "Entry released successfully.", data);
    }

    public DeviceResponse ReleaseEntryAndExit(Device device, string message)
    {
        device.ReleaseEntryAndExit(message);
        var data = new { Action = "Entry and exit released" }; // Substitua com a lógica real
        return new DeviceResponse(true, "Entry and exit released successfully.", data);
    }

    public DeviceResponse ReleaseExit(Device device, string message)
    {
        device.ReleaseExit(message);
        var data = new { Action = "Exit released" }; // Substitua com a lógica real
        return new DeviceResponse(true, "Exit released successfully.", data);
    }
}