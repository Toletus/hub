using System.Net;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Base.Utils;
using Toletus.Pack.Core.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

public static class LiteNet2Devices
{
    public static Action<LiteNet2Board> OnBoardReceived;
    public static IEnumerable<LiteNet2Board>? Boards { get; private set; }

    public static void SetBoards(ICollection<LiteNet2Board> newBoards)
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

    private static void SetRemovedBoards(ICollection<LiteNet2Board> newBoards)
    {
        Boards = Boards
            .Where(existingBoard => newBoards.Any(newBoard => newBoard.Ip.ToString() == existingBoard.Ip.ToString()))
            .ToList();
    }

    private static void SetAddedBoards(ICollection<LiteNet2Board> newBoards)
    {
        foreach (var newBoard in newBoards)
            if (!Boards.Any(existingBoard => existingBoard.Ip.ToString() == newBoard.Ip.ToString()))
                ((List<LiteNet2Board>)Boards).Add(newBoard);
    }

    public static LiteNet2Board[] SearchLiteNet2Boards(IPAddress? address = null)
    {
        address ??= NetworkInterfaceUtils.GetDefaultNetworkIPAddress();

        return LiteNetUtil.Search(address)
            .Select(LiteNet2Board.CreateFromBase).ToArray();
    }

    public static LiteNet2Board? Get(int id)
    {
        if (Boards == null || Boards.All(c => c.Id != id))
            Boards = SearchLiteNet2Boards();

        return Boards.FirstOrDefault(c => c.Id == id);
    }

    public static LiteNet2Board? Get(string ip)
    {
        if (Boards == null || Boards.All(c => c.Ip.ToString() != ip))
            Boards = SearchLiteNet2Boards();

        return Boards?.FirstOrDefault(c => c.Ip.ToString() == ip);
    }
}