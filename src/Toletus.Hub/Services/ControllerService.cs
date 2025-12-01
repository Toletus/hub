using Toletus.Hub.DeviceCollectionManager;
using Toletus.Hub.Models;
using Toletus.LiteNet1;
using Toletus.LiteNet2;
using Toletus.LiteNet3;

namespace Toletus.Hub.Services;

public class ControllerService
{
    private const string NotFoundMessage = "Device not found. Try discover.";
    private const string AlreadyConnected = "Device is already connected";
    private const string NotConnected = "Device is not connected";

    public DeviceResponse Connect(string ip, DeviceType type, string? network = null)
    {
        return type switch
        {
            DeviceType.LiteNet1 => ConnectLiteNet1(LiteNet1Devices.Get(ip)),
            DeviceType.LiteNet2 => ConnectLiteNet2(LiteNet2Devices.Get(ip)),
            DeviceType.LiteNet3 => ConnectLiteNet3(LiteNet3Devices.Get(ip), network),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public DeviceResponse ConnectSerialPort(string? portName = null)
    {
        var board = LiteNet3Board.CreateToSerialPort();
        
        board.ConnectSerialPort(portName); 
        
        LiteNet3Devices.SetBoard(board);
        
        return new DeviceResponse(Device.CreateFrom(board));
    }

    private DeviceResponse ConnectLiteNet1(LiteNet? board)
    {
        if (board == null)
            return new DeviceResponse(success: false, NotFoundMessage);

        if (board.Iniciada)
            return new DeviceResponse(success: false, AlreadyConnected);

        board.Iniciar();

        return new DeviceResponse(Device.CreateFrom(board));
    }

    private DeviceResponse ConnectLiteNet2(LiteNet2Board? board)
    {
        if (board == null)
            return new DeviceResponse(success: false, NotFoundMessage);

        if (board.Connected)
            return new DeviceResponse(success: false, AlreadyConnected);

        board.Connect();

        return new DeviceResponse(Device.CreateFrom(board));
    }

    private DeviceResponse ConnectLiteNet3(LiteNet3Board? board, string network)
    {
        if (board == null)
            return new DeviceResponse(success: false, NotFoundMessage);

        if (board.Connected)
            return new DeviceResponse(success: false, AlreadyConnected);

        board.Connect(network);

        return new DeviceResponse(Device.CreateFrom(board));
    }

    public DeviceResponse Disconnect(string ip, DeviceType type)
    {
        return type switch
        {
            DeviceType.LiteNet1 => DisconnectLiteNet1(LiteNet1Devices.Get(ip)),
            DeviceType.LiteNet2 => DisconnectLiteNet2(LiteNet2Devices.Get(ip)),
            DeviceType.LiteNet3 => DisconnectLiteNet3(LiteNet3Devices.Get(ip)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private DeviceResponse DisconnectLiteNet1(LiteNet? board)
    {
        switch (board)
        {
            case null:
                return new DeviceResponse(success: false, NotFoundMessage);
            case { Iniciada: false }:
                return new DeviceResponse(success: false, message: NotConnected);
            default:
                board!.Encerrar();

                return new DeviceResponse(Device.CreateFrom(board));
        }
    }

    private DeviceResponse DisconnectLiteNet2(LiteNet2Board? board)
    {
        switch (board)
        {
            case null:
                return new DeviceResponse(success: false, NotFoundMessage);
            case { Connected: false }:
                return new DeviceResponse(success: false, message: NotConnected);
            default:
                board!.Close();

                return new DeviceResponse(Device.CreateFrom(board));
        }
    }

    private DeviceResponse DisconnectLiteNet3(LiteNet3Board? board)
    {
        switch (board)
        {
            case null:
                return new DeviceResponse(success: false, NotFoundMessage);
            case { Connected: false }:
                return new DeviceResponse(success: false, message: NotConnected);
            default:
            {
                board!.Close();
                LiteNet3Devices.RemoveBoard(board);
            }

                return new DeviceResponse(Device.CreateFrom(board));
        }
    }
}