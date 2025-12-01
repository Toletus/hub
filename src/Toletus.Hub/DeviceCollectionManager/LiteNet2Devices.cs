using System.Net;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Base.Utils;
using Toletus.Pack.Core.Network.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

public static class LiteNet2Devices
{
    public static Action<LiteNet2Board> OnBoardReceived;
    public static List<LiteNet2Board>? Boards { get; private set; }

    public static void SetBoards(List<LiteNet2Board> newBoards)
    {
        if (Boards == null)
            Boards = newBoards;
        else
        {
            SetRemovedBoards(newBoards);
            SetAddedBoards(newBoards);
        }

        foreach (var newBoard in newBoards)
            OnBoardReceived?.Invoke(newBoard);
    }

    private static void SetRemovedBoards(List<LiteNet2Board> newBoards)
    {
        var removedBoards = Boards?.Where(x => 
                newBoards.Exists(p => x.Ip.ToString() == p.Ip.ToString()))
            .ToList();
        
        if (removedBoards == null || removedBoards.Count == 0) return;
        
        Boards?.RemoveAll(removedBoards.Contains);
    }

    private static void SetAddedBoards(List<LiteNet2Board> newBoards)
    {
        foreach (var newBoard in newBoards)
            if (!Boards.Any(existingBoard => existingBoard.Ip.ToString() == newBoard.Ip.ToString()))
                Boards.Add(newBoard);
    }

    public static LiteNet2Board[] SearchLiteNet2Boards(IPAddress? address = null)
    {
        address ??= NetworkInterfaceUtils.GetDefaultNetworkIPAddress();

        return LiteNetUtil.Search(address)
            .Select(LiteNet2Board.CreateFromBase).ToArray();
    }

    public static LiteNet2Board? Get(int id) => Boards.FirstOrDefault(c => c.Id == id);

    public static LiteNet2Board? Get(string ip) => Boards?.FirstOrDefault(c => c.Ip.ToString() == ip);
}