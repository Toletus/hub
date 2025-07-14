using System.Net;
using Toletus.LiteNet3;
using Toletus.Pack.Core.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

public static class LiteNet3Devices
{
    public static Action<LiteNet3Board> OnBoardReceived;
    public static IEnumerable<LiteNet3Board>? Boards { get; private set; }

    public static void SetBoards(ICollection<LiteNet3Board> newBoards)
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

    private static void SetRemovedBoards(ICollection<LiteNet3Board> newBoards)
    {
        Boards = Boards
            .Where(existingBoard => newBoards.Any(newBoard => newBoard.Ip.ToString() == existingBoard.Ip.ToString()))
            .ToList();
    }

    private static void SetAddedBoards(ICollection<LiteNet3Board> newBoards)
    {
        foreach (var newBoard in newBoards)
            if (!Boards.Any(existingBoard => existingBoard.Ip.ToString() == newBoard.Ip.ToString()))
                ((List<LiteNet3Board>)Boards).Add(newBoard);
    }

    public static LiteNet3Board[] SearchLiteNet3Boards(IPAddress? address = null)
    {
        address ??= NetworkInterfaceUtils.GetDefaultNetworkIPAddress();

        return LiteNetUtil.Search(address)
            .Select(LiteNet3Board.CreateFromBase).ToArray();
    }

    public static LiteNet3Board? Get(int id)
    {
        if (Boards == null || Boards.All(c => c.Id != id))
            Boards = SearchLiteNet3Boards();

        return Boards.FirstOrDefault(c => c.Id == id);
    }

    public static LiteNet3Board? Get(string ip)
    {
        if (Boards == null || Boards.All(c => c.Ip.ToString() != ip))
            Boards = SearchLiteNet3Boards();

        return Boards?.FirstOrDefault(c => c.Ip.ToString() == ip);
    }
}