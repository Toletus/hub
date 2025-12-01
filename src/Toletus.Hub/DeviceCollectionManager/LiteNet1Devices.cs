using System.Net;
using Toletus.LiteNet1;
using Toletus.Pack.Core.Network.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

public static class LiteNet1Devices
{
    public static Action<LiteNet, Controlador> OnBoardReceived;
    private static readonly Controlador Controlador = new();
    public static List<LiteNet>? Boards { get; private set; }

    public static void SetBoards(List<LiteNet> newBoards)
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

    private static void SetRemovedBoards(List<LiteNet> newBoards)
    {
        var removedBoards = Boards?.Where(x => 
                newBoards.Exists(p => x.IP.ToString() == p.IP.ToString()))
            .ToList();
        
        if (removedBoards == null || removedBoards.Count == 0) return;
        
        Boards?.RemoveAll(removedBoards.Contains);
    }

    private static void SetAddedBoards(List<LiteNet> newBoards)
    {
        foreach (var newBoard in newBoards)
            if (Boards.All(existingBoard => existingBoard.IP != newBoard.IP))
                Boards.Add(newBoard);
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

    public static LiteNet? Get(int id) => Boards.FirstOrDefault(c => c.Id == id);

    public static LiteNet? Get(string ip) => Boards?.FirstOrDefault(c => c.IP == ip);
}