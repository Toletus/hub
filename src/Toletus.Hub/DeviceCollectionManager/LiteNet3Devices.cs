using System.Net;
using Toletus.LiteNet3;
using Toletus.Pack.Core.Network.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

public static class LiteNet3Devices
{
    public static Action<LiteNet3Board> OnBoardReceived;
    public static List<LiteNet3Board>? Boards { get; private set; }

    public static void SetBoards(List<LiteNet3Board> newBoards)
    {
        if (Boards == null)
            Boards = newBoards.ToList();
        else
        {
            SetRemovedBoards(newBoards);
            SetAddedBoards(newBoards);
        }

        foreach (var newBoard in newBoards)
            OnBoardReceived?.Invoke(newBoard);
    }

    public static void SetBoard(LiteNet3Board board)
        => SetBoards([board]);

    public static void RemoveBoard(LiteNet3Board board)
    {
        var removedBoard = Boards?.FirstOrDefault(x => x.Ip.ToString() == board.Ip.ToString());
        if (removedBoard != null) Boards?.Remove(removedBoard);
    }

    private static void SetRemovedBoards(List<LiteNet3Board> newBoards)
    {
        var removedBoards = Boards?.Where(x => 
                newBoards.Exists(p => x.Ip.ToString() == p.Ip.ToString()))
            .ToList();
        
        if (removedBoards == null || removedBoards.Count == 0) return;
        
        Boards?.RemoveAll(removedBoards.Contains);
    }

    private static void SetAddedBoards(List<LiteNet3Board> newBoards)
    {
        foreach (var newBoard in newBoards)
            if (!Boards.Any(existingBoard => existingBoard.Ip.ToString() == newBoard.Ip.ToString()))
                Boards.Add(newBoard);
    }

    public static LiteNet3Board[] SearchLiteNet3Boards(IPAddress? address = null)
    {
        address ??= NetworkInterfaceUtils.GetDefaultNetworkIPAddress();

        return LiteNetUtil.Search(address)
            .Select(LiteNet3Board.CreateFromBase).ToArray();
    }

    public static LiteNet3Board? Get(int id) => Boards.FirstOrDefault(c => c.Id == id);

    public static LiteNet3Board? Get(string ip) => Boards?.FirstOrDefault(c => c.Ip.ToString() == ip);
}