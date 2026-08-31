using System.Net;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Base;
using Toletus.LiteNet2.Base.Utils;
using Toletus.Pack.Core.Network.Utils;

namespace Toletus.Hub.DeviceCollectionManager;

/// <summary>
/// Registro de equipamentos LiteNet2 — fonte de verdade por identidade (serial), thread-safe.
/// Mesclagem NÃO destrutiva das varreduras (ADR-002 / TDD §4.4): instâncias conectadas
/// nunca são trocadas por instâncias novas sem conexão (fim da varredura destrutiva).
/// </summary>
public static class LiteNet2Devices
{
    public static Action<string>? Log;
    public static Action<LiteNet2Board>? OnBoardReceived;

    private static readonly object Lock = new();
    private static readonly List<LiteNet2Board> BoardsInternal = new();

    /// <summary>Snapshot para leitura concorrente segura durante a varredura (enumerar não lança).</summary>
    public static List<LiteNet2Board>? Boards
    {
        get
        {
            lock (Lock) return BoardsInternal.ToList();
        }
    }

    public static void SetBoards(List<LiteNet2Board> scanned)
    {
        var newlyAdded = new List<LiteNet2Board>();

        lock (Lock)
        {
            var present = new HashSet<LiteNet2Board>();

            foreach (var s in scanned)
            {
                var existing = FindByIdentity(s);

                if (existing != null)
                {
                    present.Add(existing);

                    if (existing.Connected)
                    {
                        // identidade conhecida, conectada -> mantém instância; atualiza metadados in place.
                        existing.Ip = s.Ip;
                        if (!string.IsNullOrEmpty(s.ConnectionInfo))
                            existing.ConnectionInfo = s.ConnectionInfo;
                    }
                    else
                    {
                        // identidade conhecida, desconectada -> substitui pela nova.
                        BoardsInternal.Remove(existing);
                        BoardsInternal.Add(s);
                        present.Remove(existing);
                        present.Add(s);
                        newlyAdded.Add(s);
                    }

                    continue;
                }

                var sameAddress = BoardsInternal.FirstOrDefault(b => b.Ip.Equals(s.Ip));
                if (sameAddress != null)
                {
                    if (sameAddress.Connected)
                    {
                        // mesmo endereço, identidade divergente, conectada -> mantém a conectada + log.
                        present.Add(sameAddress);
                        Log?.Invoke(
                            $"[LiteNet2Devices] Divergência no endereço {s.Ip}: mantida a conectada " +
                            $"(serial {sameAddress.SerialNumber}); nova (serial {s.SerialNumber}) ignorada. " +
                            "Troca física será detectada pelo keepalive.");
                        continue;
                    }

                    // mesmo endereço, desconectada -> substitui.
                    BoardsInternal.Remove(sameAddress);
                }

                // identidade nova -> adiciona.
                BoardsInternal.Add(s);
                present.Add(s);
                newlyAdded.Add(s);
            }

            // Ausentes da varredura: desconectada -> remove; conectada -> mantém (o transporte decide a queda).
            foreach (var board in BoardsInternal.ToList())
            {
                if (present.Contains(board)) continue;
                if (!board.Connected)
                    BoardsInternal.Remove(board);
            }
        }

        // OnBoardReceived só para as instâncias NOVAS — evita re-subscrição de handlers nas mantidas (AC-007.2/007.3).
        foreach (var board in newlyAdded)
            OnBoardReceived?.Invoke(board);
    }

    private static LiteNet2Board? FindByIdentity(LiteNet2Board scanned)
    {
        if (!string.IsNullOrEmpty(scanned.SerialNumber))
        {
            var bySerial = BoardsInternal.FirstOrDefault(b => b.SerialNumber == scanned.SerialNumber);
            if (bySerial != null) return bySerial;
            return null; // tem serial mas não bate: trata como identidade nova / divergência de endereço.
        }

        // sem serial (ex.: endereço já conectado, identidade veio do registro) -> casa por endereço.
        return BoardsInternal.FirstOrDefault(b => b.Ip.Equals(scanned.Ip));
    }

    /// <summary>Esvazia o registro (uso operacional/teste).</summary>
    public static void Clear()
    {
        lock (Lock) BoardsInternal.Clear();
    }

    public static LiteNet2Board[] SearchLiteNet2Boards(IPAddress? address = null)
    {
        address ??= NetworkInterfaceUtils.GetDefaultNetworkIPAddress();

        // Exclui os endereços já conectados — a varredura não abre nova conexão a eles (AC-002.3).
        var exclusions = Boards?
            .Where(b => b.Connected)
            .Select(b => b.Ip)
            .ToList();

        return (LiteNetUtil.Search(address, exclusions) ?? new List<LiteNet2BoardBase>())
            .Select(LiteNet2Board.CreateFromBase).ToArray();
    }

    public static LiteNet2Board? Get(int id)
    {
        lock (Lock) return BoardsInternal.FirstOrDefault(c => c.Id == id);
    }

    public static LiteNet2Board? Get(string ip)
    {
        lock (Lock) return BoardsInternal.FirstOrDefault(c => c.Ip.ToString() == ip);
    }
}
