using Microsoft.AspNetCore.Mvc;
using Toletus.Hub.Models;
using Toletus.Hub.Services;

namespace Toletus.Hub.Controllers;

/// <summary>
/// Controller for managing network and device operations.
/// </summary>
public class DeviceConnectionController(
    DeviceService deviceService,
    ControllerService controllerService)
    : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceConnectionController"/> class.
    /// </summary>
    /// <param name="deviceAppService">Service for handling device-related operations.</param>
    /// <param name="controllerAppService">Service for managing device connections and disconnections.</param>
    /// <summary>
    /// Retrieves the names of all available networks.
    /// </summary>
    /// <returns>A collection of network names.</returns>
    [HttpGet(nameof(GetNetworks))]
    public IEnumerable<string> GetNetworks()
    {
        return deviceService.GetNetworksNames();
    }

    /// <summary>
    /// Retrieves the default network name.
    /// </summary>
    /// <returns>The default network name as a string.</returns>
    [HttpGet(nameof(GetDefaultNetworkName))]
    public string GetDefaultNetworkName()
    {
        return deviceService.GetDefaultNetworkName();
    }

    /// <summary>
    /// Discovers devices on the specified network or on the default network if not provided.
    /// </summary>
    /// <param name="network">The name of the network to discover devices on. Optional.</param>
    /// <returns>A <see cref="DeviceResponse{T}"/> containing a collection of discovered devices.</returns>
    [HttpGet(nameof(DiscoverDevices))]
    public IActionResult DiscoverDevices(string? network = null)
    {
        return GetResponse(deviceService.DiscoverDevices(network));
    }

    /// <summary>
    /// Retrieves a list of devices on the specified network or on the default network if not provided.
    /// </summary>
    /// <param name="network">The name of the network to get devices from. Optional.</param>
    /// <returns>A <see cref="DeviceResponse{T}"/> containing a collection of devices.</returns>
    [HttpGet(nameof(GetDevices))]
    public IActionResult GetDevices(string? network = null)
    {
        return GetResponse(deviceService.GetDevices(network));
    }

    /// <summary>
    /// Connects to a device using its IP address.
    /// </summary>
    /// <param name="ip">The IP address of the device to connect to.</param>
    /// <param name="type"></param>
    /// <param name="network"></param>
    /// <returns>A <see cref="DeviceResponse{T}"/> containing the connected device.</returns>
    [HttpPost(nameof(Connect))]
    public IActionResult Connect(string ip, DeviceType type, string? network = null)
    {
        return GetResponse(controllerService.Connect(ip, type, network));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="portName"></param>
    /// <returns></returns>
    [HttpPost(nameof(ConnectToSerialPort))]
    public IActionResult ConnectToSerialPort(string? portName = null)
    {
        return GetResponse(controllerService.ConnectSerialPort(portName));
    }

    /// <summary>
    /// Disconnects from a device using its IP address.
    /// </summary>
    /// <param name="ip">The IP address of the device to disconnect from.</param>
    /// <param name="type"></param>
    /// <returns>A <see cref="DeviceResponse{T}"/> containing the disconnected device.</returns>
    [HttpPost(nameof(Disconnect))]
    public IActionResult Disconnect(string ip, DeviceType type)
    {
        return GetResponse(controllerService.Disconnect(ip, type));
    }
}