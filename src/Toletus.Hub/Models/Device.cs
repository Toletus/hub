using Toletus.Hub.DeviceCollectionManager;
using Toletus.LiteNet1;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Base;
using Toletus.LiteNet3;

namespace Toletus.Hub.Models;

public partial class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int Port { get; set; }
    public DeviceType Type { get; set; }

    public bool Connected { get; set; }

    // Supondo que ambos os boards herdem de uma classe base comum
    private object _board;

    public override string ToString() => $"{base.ToString()} {Name}";

    public static Device? AsDevice(object data) =>
        data is not Device device ? null : device;

    public static IEnumerable<Device>? AsDevices(object data) =>
        data is not IEnumerable<Device> devices ? null : devices;

    public T? Get<T>() where T : class
    {
        return typeof(T) switch
        {
            { } t when t == typeof(LiteNet) =>
                LiteNet1Devices.Boards?.FirstOrDefault(x => x.IP == Ip) as T,
            { } t when t == typeof(LiteNet2Board) =>
                LiteNet2Devices.Boards?.FirstOrDefault(x => x.Ip.ToString() == Ip) as T,
            { } t when t == typeof(LiteNet3Board) =>
                LiteNet3Devices.Boards?.FirstOrDefault(x => x.Ip.ToString() == Ip) as T,
            _ => null // Caso padrão para tipos que não são suportados
        };
    }

    public object? Get()
    {
        return Type switch
        {
            DeviceType.LiteNet1 => LiteNet1Devices.Boards?.FirstOrDefault(x => x.IP == Ip),
            DeviceType.LiteNet2 => LiteNet2Devices.Boards?.FirstOrDefault(x => x.Ip.ToString() == Ip),
            DeviceType.LiteNet3 => LiteNet3Devices.Boards?.FirstOrDefault(x => x.Ip.ToString() == Ip),
            _ => null
        };
    }

    public static Device CreateFrom(LiteNet board)
    {
        return new Device
        {
            Id = board.Id,
            Name = $"{nameof(DeviceType.LiteNet1)} #{board.Id}",
            Ip = board.IP,
            Port = board.Porta,
            Type = DeviceType.LiteNet1,
            Connected = board.Iniciada,
            _board = board
        };
    }


    public static Device CreateFrom(LiteNet2Board board)
    {
        return new Device
        {
            Id = board.Id,
            Name = $"{nameof(DeviceType.LiteNet2)} #{board.Id}",
            Ip = board.Ip.ToString(),
            SerialNumber = board.SerialNumber!,
            Port = LiteNet2BoardBase.Port,
            Type = DeviceType.LiteNet2,
            Connected = board.Connected,
            _board = board
        };
    }

    public static Device CreateFrom(LiteNet3Board board)
    {
        return new Device
        {
            Id = board.Id,
            Name = string.IsNullOrEmpty(board.Alias)
                ? $"{nameof(DeviceType.LiteNet3)} #{board.Id}"
                : $"{board.Alias} #{board.Id}",
            Ip = board.Ip.ToString(),
            SerialNumber = board.Serial!,
            Port = LiteNet3BoardBase.Port,
            Type = DeviceType.LiteNet3,
            Connected = board.Connected,
            _board = board
        };
    }
}