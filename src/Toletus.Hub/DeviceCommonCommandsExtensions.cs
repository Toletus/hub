using Toletus.Hub.Models;
using Toletus.LiteNet1;
using Toletus.LiteNet2;
using Toletus.LiteNet3;

namespace Toletus.Hub;

public static class DeviceCommonCommandsExtensions
{
    public static void ReleaseEntry(this Device device, string message = "")
    {
        switch (device.Get())
        {
            case LiteNet liteNetBoard:
                liteNetBoard.LiberarEntrada(message);
                break;
            case LiteNet2Board liteNet2Board:
                liteNet2Board.ReleaseEntry(message);
                break;
            case LiteNet3Board liteNet3Board:
                liteNet3Board.ReleaseEntry("", message);
                break;
            default:
                throw new InvalidOperationException("Board type not supported for this operation.");
        }
    }

    public static void ReleaseExit(this Device device, string message = "")
    {
        switch (device.Get())
        {
            case LiteNet liteNetBoard:
                liteNetBoard.LiberarSaida(message);
                break;
            case LiteNet2Board liteNet2Board:
                liteNet2Board.ReleaseExit(message);
                break;

            case LiteNet3Board liteNet3Board:
                liteNet3Board.ReleaseExit("", message);
                break;
            default:
                throw new InvalidOperationException("Board type not supported for this operation.");
        }
    }

    public static void ReleaseEntryAndExit(this Device device, string message = "")
    {
        switch (device.Get())
        {
            case LiteNet2Board liteNet2Board:
                liteNet2Board.ReleaseEntryAndExit(message);
                break;
            case LiteNet3Board liteNet3Board:
                liteNet3Board.ReleaseEntryAndExit("", message);
                break;
            default:
                throw new InvalidOperationException("Board type not supported for this operation.");
        }
    }
}