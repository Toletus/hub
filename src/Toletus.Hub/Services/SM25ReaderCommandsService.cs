using System.Collections.Concurrent;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices;
using Toletus.LiteNet2;
using Toletus.SM25;
using Toletus.SM25.Command.Enums;

namespace Toletus.Hub.Services;

// ReSharper disable once InconsistentNaming
public class SM25ReaderCommandsService : SM25NotificationService
{
    // Concorrente: os comandos chegam por caminhos assíncronos e o dicionário comum
    // que havia aqui podia ser mutado por dois deles ao mesmo tempo. O cache de
    // placas ao lado (LiteNet2) já era concorrente; este não era.
    private static readonly ConcurrentDictionary<string, ReaderEntry> Readers =
        new(StringComparer.OrdinalIgnoreCase);

    // Um portão por leitor: duas operações lógicas nunca compartilham o mesmo
    // equipamento. Vive fora de Readers porque precisa sobreviver à entrada ser
    // removida no fim da sessão.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates =
        new(StringComparer.OrdinalIgnoreCase);

    // internal, não private: o construtor da sessão precisa recebê-lo. Continua
    // invisível para quem consome o pacote.
    internal sealed class ReaderEntry(SM25Reader reader)
    {
        public SM25Reader Reader { get; } = reader;

        /// <summary>
        /// Verdadeiro enquanto uma sessão segura este leitor. Com sessão aberta, os
        /// comandos individuais não abrem nem fecham conexão: reaproveitam a dela.
        /// </summary>
        public bool HeldBySession { get; set; }
    }

    #region Reads Commands

    public async Task<Notification> GetDeviceName(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetDeviceName,
            () => reader?.Sync.GetDeviceName());
        Disconnect(device);

        return notification;
    }

    // ReSharper disable once InconsistentNaming
    public async Task<Notification> GetFWVersion(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetFWVersion,
            () => reader?.Sync.GetFWVersion());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetDeviceId(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetDeviceID,
            () => reader?.Sync.GetDeviceId());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetEmptyId(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetEmptyID,
            () => reader?.Sync.GetEmptyID());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetEnrollData(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetEnrollData,
            () => reader?.Sync.GetEnrollData());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetEnrollCount(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetEnrollCount,
            () => reader?.Sync.GetEnrollCount());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetTemplateStatus(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetTemplateStatus,
            () => reader?.Sync.GetTemplateStatus(id));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetDuplicationCheck(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetDuplicationCheck,
            () => reader?.Sync.GetDuplicationCheck());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetSecurityLevel(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetSecurityLevel,
            () => reader?.Sync.GetSecurityLevel());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> GetFingerTimeOut(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.GetFingerTimeOut,
            () => reader?.Sync.GetFingerTimeOut());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> ReadTemplate(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.ReadTemplate,
            () => reader?.Sync.ReadTemplate(id));

        return notification;
    }

    #endregion

    #region Writes Commands

    public async Task<Notification> Enroll(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.Enroll,
            () => reader?.Sync.Enroll(id));

        return notification;
    }

    // ReSharper disable once InconsistentNaming
    public async Task<Notification> EnrollAndStoreinRAM(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.EnrollAndStoreinRAM,
            () => reader?.Sync.EnrollAndStoreinRAM());

        return notification;
    }

    public async Task<Notification> ClearTemplate(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.ClearTemplate,
            () => reader?.Sync.ClearTemplate(id));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> ClearAllTemplate(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.ClearAllTemplate,
            () => reader?.Sync.ClearAllTemplate());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetDeviceId(Device device, ushort id)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetDeviceID,
            () => reader?.Sync.SetDeviceId(id));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetFingerTimeOut(Device device, ushort timeOut)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetFingerTimeOut,
            () => reader?.Sync.SetFingerTimeOut(timeOut));
        Disconnect(device);

        return notification;
    }

    // ReSharper disable once InconsistentNaming
    public async Task<Notification> FPCancel(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.FPCancel,
            () => reader?.Sync.FPCancel());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetDuplicationCheck(Device device, bool check)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetDuplicationCheck,
            () => reader?.Sync.SetDuplicationCheck(check));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> SetSecurityLevel(Device device, ushort level)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.SetSecurityLevel,
            () => reader?.Sync.SetSecurityLevel(level));
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> WriteTemplate(Device device)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.WriteTemplate,
            () => reader?.Sync.WriteTemplate());
        Disconnect(device);

        return notification;
    }

    public async Task<Notification> WriteTemplateData(Device device, ushort id, byte[] template)
    {
        var reader = Connect(device);
        var notification = await ExecuteCommandAsync(
            device,
            SM25Commands.WriteTemplate,
            () => reader?.Sync.WriteTemplateData(id, template));
        Disconnect(device);

        return notification;
    }

    #endregion

    #region Session

    /// <summary>
    /// Abre uma sessão exclusiva com o leitor daquele equipamento e a mantém até o
    /// descarte. Enquanto ela vive, os comandos individuais reaproveitam a mesma
    /// conexão em vez de abrir e fechar uma por comando.
    ///
    /// <para>
    /// Existe porque duas operações do leitor são <b>sequências</b>, não comandos
    /// avulsos: gravar um template é anunciar o tamanho, esperar a confirmação e só
    /// então enviar slot e dados, tudo na mesma sessão; e capturar é uma interação
    /// dirigida por eventos que dura até o tempo limite do dedo. Com conexão por
    /// comando, a gravação parte em duas conexões e a captura perde os eventos ao
    /// fim do primeiro comando.
    /// </para>
    ///
    /// <para>
    /// A exclusividade é por leitor: leitores diferentes seguem independentes. A
    /// espera é pelo portão daquele equipamento, então duas sessões no mesmo leitor
    /// se enfileiram em vez de se corromperem.
    /// </para>
    ///
    /// <para>
    /// <b>Sessão não é vaga de teto de carga.</b> Quem limita quantas requisições
    /// simultâneas a máquina faz é o chamador, por requisição — segurar uma vaga pela
    /// duração da sessão travaria as demais sincronizações.
    /// </para>
    /// </summary>
    /// <returns>
    /// A sessão, ou <c>null</c> quando não há placa conectada para aquele equipamento.
    /// Quando devolve <c>null</c>, nada foi adquirido e não há o que descartar.
    /// </returns>
    public static async Task<SM25ReaderSession?> OpenSessionAsync(
        Device device, CancellationToken cancellationToken = default)
    {
        var key = ResolveKey(device);

        if (key == null) return null;

        var gate = GateFor(key);
        await gate.WaitAsync(cancellationToken);

        try
        {
            var reader = GetReader(device);

            if (reader == null)
            {
                gate.Release();
                return null;
            }

            reader.Connect();

            var entry = new ReaderEntry(reader) { HeldBySession = true };
            Readers[key] = entry;
            SubscribeSM25Reader(reader);

            return new SM25ReaderSession(key, entry, gate);
        }
        catch
        {
            gate.Release();
            throw;
        }
    }

    /// <summary>
    /// Sessão exclusiva com o leitor de um equipamento. Descartar fecha a conexão e
    /// libera o leitor para a próxima operação — inclusive quando a operação falhou.
    /// </summary>
    public sealed class SM25ReaderSession : IDisposable
    {
        private readonly string _key;
        private readonly ReaderEntry _entry;
        private readonly SemaphoreSlim _gate;
        private int _disposed;

        internal SM25ReaderSession(string key, ReaderEntry entry, SemaphoreSlim gate)
        {
            _key = key;
            _entry = entry;
            _gate = gate;
        }

        /// <summary>O leitor desta sessão, já conectado.</summary>
        public SM25Reader Reader => _entry.Reader;

        public void Dispose()
        {
            // Descarte duplo não pode liberar o portão duas vezes: isso deixaria dois
            // donos no mesmo leitor.
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            try
            {
                _entry.HeldBySession = false;
                Readers.TryRemove(_key, out _);

                try
                {
                    _entry.Reader.Close();
                }
                finally
                {
                    UnsubscribeSM25Reader(_entry.Reader);
                }
            }
            finally
            {
                _gate.Release();
            }
        }
    }

    #endregion

    #region Privates Methods

    /// <summary>
    /// Chave do leitor no cache. Vem do endereço da <b>placa</b>, que é o mesmo
    /// endereço usado para criar o leitor — antes a chave vinha do dispositivo e a
    /// criação da placa, e as duas origens podiam divergir, vazando entrada no cache.
    /// <c>null</c> quando não há placa conectada: sem placa não há leitor.
    /// </summary>
    private static string? ResolveKey(Device device)
    {
        var board = device.Get<LiteNet2Board>();

        return board is not { Connected: true } ? null : board.Ip.ToString();
    }

    private static SM25Reader? GetReader(Device device)
    {
        var board = device.Get<LiteNet2Board>();

        return board is not { Connected: true } ? null : new SM25Reader(board.Ip);
    }

    private static SM25Reader? Connect(Device device)
    {
        var key = ResolveKey(device);

        if (key == null) return null;

        if (Readers.TryGetValue(key, out var entry))
        {
            // Com sessão aberta o leitor é dela: devolve como está, mesmo que a
            // conexão tenha caído — reconectar por baixo da sessão criaria um
            // segundo socket para o mesmo leitor.
            if (entry.HeldBySession) return entry.Reader;

            if (entry.Reader.Connected) return entry.Reader;
        }

        var reader = GetReader(device);

        if (reader == null) return null;

        reader.Connect();
        Readers[key] = new ReaderEntry(reader);
        SubscribeSM25Reader(reader);
        return reader;
    }

    private static void Disconnect(Device device)
    {
        var key = ResolveKey(device);

        if (key == null) return;

        if (!Readers.TryGetValue(key, out var entry))
            return; // Nada a fechar. Antes fabricava um leitor só para fechá-lo.

        // Sessão aberta: o fechamento é dela, no fim da operação lógica.
        if (entry.HeldBySession) return;

        Readers.TryRemove(key, out _);
        entry.Reader.Close();
        UnsubscribeSM25Reader(entry.Reader);
    }

    private static SemaphoreSlim GateFor(string key) =>
        Gates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

    #endregion
}