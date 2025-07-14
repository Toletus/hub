using System.Net;
using Toletus.LiteNet1;
using Toletus.Pack.Core.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

public static class LiteNet1Devices
{
    public static Action<LiteNet, Controlador> OnBoardReceived;
    private static readonly Controlador Controlador = new();
    public static IEnumerable<LiteNet>? Boards { get; private set; }

    public static void SetBoards(ICollection<LiteNet> newBoards)
    {
        if (Boards == null)
            Boards = newBoards;
        else
        {
            SetRemovedBoards(newBoards);
            SetAddedBoards(newBoards);
        }

        foreach (var newBoard in newBoards)
            OnBoardReceived?.Invoke(newBoard, Controlador);
    }

    private static void SetRemovedBoards(ICollection<LiteNet> newBoards)
    {
        Boards = Boards
            .Where(existingBoard => newBoards.Any(newBoard => newBoard.IP == existingBoard.IP))
            .ToList();
    }

    private static void SetAddedBoards(ICollection<LiteNet> newBoards)
    {
        foreach (var newBoard in newBoards)
            if (Boards.All(existingBoard => existingBoard.IP != newBoard.IP))
                ((List<LiteNet>)Boards).Add(newBoard);
    }

    public static LiteNet[] SearchLiteNetBoards(IPAddress? address = null)
    {
        try
        {
            address ??= NetworkInterfaceUtils.GetDefaultNetworkIPAddress();

            var waitHandle = new AutoResetEvent(false);
            Controlador.Carregar(address.ToString(), waitHandle);
            waitHandle.WaitOne();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return Controlador.LiteNets.ToArray();
    }

    public static LiteNet? Get(int id)
    {
        if (Boards == null || Boards.All(c => c.Id != id))
            Boards = SearchLiteNetBoards();

        return Boards.FirstOrDefault(c => c.Id == id);
    }

    public static LiteNet? Get(string ip)
    {
        if (Boards == null || Boards.All(c => c.IP != ip))
            Boards = SearchLiteNetBoards();

        return Boards?.FirstOrDefault(c => c.IP == ip);
    }
}