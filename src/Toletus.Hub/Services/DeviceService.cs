using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Models;
using Toletus.Pack.Core.Network.Utils;

namespace Toletus.Hub.Services;

/// <summary>
/// Provides services for managing and discovering devices on a network.
/// </summary>
public class DeviceService
{
    /// <summary>
    /// Retrieves the names of all available network interfaces.
    /// </summary>
    /// <returns>An enumerable collection of network interface names.</returns>
    public IEnumerable<string> GetNetworksNames()
    {
        var networks =
            NetworkInterfaceUtils.GetNetworkInterfaces().Select(c => c.Key);

        return networks;
    }

    public string? GetDefaultNetworkName()
    {
        return NetworkInterfaceUtils.GetDefaultNetworkInterface()?.Name;
    }

    public static IEnumerable<Device>? Devices => GetDevices();

    private static IEnumerable<Device>? GetDevices()
    {
        var liteNet2 = LiteNet2Devices.Boards?.Select(Device.CreateFrom);
        var liteNet3 = LiteNet3Devices.Boards?.Select(Device.CreateFrom);
        var liteNet1 = LiteNet1Devices.Boards?.Select(Device.CreateFrom);

        var conjuntos = new[] { liteNet2, liteNet3, liteNet1 }
            .Where(sequence => sequence != null)
            .Cast<IEnumerable<Device>>()
            .ToArray();

        if (conjuntos.Length == 0)
            return null;

        return conjuntos
            .Skip(1)
            .Aggregate(
                conjuntos[0],
                (acc, next) => acc.Concat(next));
    }

    /// <summary>
    /// Retrieves the discovered devices for a given network.
    /// If no devices are cached, it discovers them using the specified network.
    /// </summary>
    /// <param name="network">The name of the network to discover devices on.</param>
    /// <returns>An enumerable collection of <see cref="Device"/> objects.</returns>
    public DeviceResponse GetDevices(string? network)
    {
        network ??= GetDefaultNetworkName();
        return Devices == null || !Devices.Any() ? DiscoverDevices(network) : new DeviceResponse(Devices);
    }

    /// <summary>
    /// Discovers devices on the specified network by searching for connected LiteNet devices.
    /// </summary>
    /// <param name="network">The name of the network to perform the discovery on.</param>
    /// <returns>An enumerable collection of discovered <see cref="Device"/> objects.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the specified network is not found in the network interfaces.</exception>
    public DeviceResponse DiscoverDevices(string? network)
    {
        network ??= GetDefaultNetworkName();

        var networks =
            NetworkInterfaceUtils.GetNetworkInterfaces();

        if (!networks.TryGetValue(network, out _))
            return new DeviceResponse(success: false,
                message: $"The specified network '{network}' was not found.");

        LiteNet1Devices.SetBoards(LiteNet1Devices.SearchLiteNetBoards(networks[network]).ToList());
        LiteNet2Devices.SetBoards(LiteNet2Devices.SearchLiteNet2Boards(networks[network]).ToList());
        LiteNet3Devices.SetBoards(LiteNet3Devices.SearchLiteNet3Boards(networks[network]).ToList());

        return new DeviceResponse(Devices);
    }
}