using Toletus.Hub.Models;

namespace Toletus.Hub.Services
{
    public class BasicCommonCommandService()
    {
        public DeviceResponse GetId(Device device)
        {
            var data = new { DeviceId = device.Id };
            return new DeviceResponse(true, "Device ID retrieved successfully.", data);
        }

        public DeviceResponse SetId(Device device)
        {
            var data = new { NewDeviceId = device.Id }; // Substitua com a lógica real
            return new DeviceResponse(true, "Device ID set successfully.", data);
        }

        public DeviceResponse ReleaseEntry(Device device)
        {
            device.ReleaseEntry();
            var data = new { Action = "Entry released" }; // Substitua com a lógica real
            return new DeviceResponse(true, "Entry released successfully.", data);
        }

        public DeviceResponse ReleaseEntryAndExit(Device device)
        {
            device.ReleaseEntryAndExit();
            var data = new { Action = "Entry and exit released" }; // Substitua com a lógica real
            return new DeviceResponse(true, "Entry and exit released successfully.", data);
        }

        public DeviceResponse ReleaseExit(Device device)
        {
            device.ReleaseExit();
            var data = new { Action = "Exit released" }; // Substitua com a lógica real
            return new DeviceResponse(true, "Exit released successfully.", data);
        }

        public DeviceResponse SetEntryClockwise(Device device)
        {
            var data = new { EntryClockwiseSet = true }; // Substitua com a lógica real
            return new DeviceResponse(true, "Entry clockwise set successfully.", data);
        }

        public DeviceResponse SetFlowControl(Device device)
        {
            var data = new { FlowControlSet = true }; // Substitua com a lógica real
            return new DeviceResponse(true, "Flow control set successfully.", data);
        }
    }
}