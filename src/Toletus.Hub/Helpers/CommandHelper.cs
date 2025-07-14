using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.LiteNet2;
using Toletus.LiteNet2.Command;
using Toletus.LiteNet2.Command.Enums;
using Toletus.LiteNet3.Handler.Responses.ActionsResponse;
using Toletus.LiteNet3.Handler.Responses.Base;
using Toletus.LiteNet3.Handler.Responses.FetchesResponse;
using Toletus.LiteNet3.Handler.Responses.NotificationsResponses;
using Toletus.LiteNet3.Handler.Responses.UpdatesResponse;

namespace Toletus.Hub.Helpers;

public static class CommandHelper
{
    public static Dictionary<LiteNet2Commands, Func<TInput, TOutput>> CreateCommandMap<TInput, TOutput>(
        string? ip = null, int? id = null, int? command = null, DeviceType? type = null)
    {
        if (typeof(TInput) == typeof(LiteNet2Board) && typeof(TOutput) == typeof(object))
            return CreateCommandBoardMap<TInput, TOutput>();

        if (typeof(TInput) == typeof(LiteNet2Response) && typeof(TOutput) == typeof(Notification))
            return CreateCommandResponseMap<TInput, TOutput>(ip!, id!.Value, command!.Value, type!.Value);

        return null;
    }
    
    public static int GetCommand(object response)
        => response switch
        {
            BuzzerResponse
                or BuzzerUpdateResponse
                or BuzzerActionResponse
                => (int)ResponseType.Buzzer,

            DiscoveryResponse
                or DiscoveryUpdateResponse
                => (int)ResponseType.Discovery,

            DisplayResponse
                or DisplayUpdateResponse
                or DisplayActionResponse
                => (int)ResponseType.Display,

            EthernetResponse
                or EthernetUpdateResponse
                => (int)ResponseType.Ethernet,

            FlowResponse
                or FlowUpdateResponse
                => (int)ResponseType.Flow,

            LiteNet3Response
                or LiteNet3UpdateResponse
                or LiteNet3ActionResponse
                => (int)ResponseType.LiteNet3,

            SensorResponse
                or SensorUpdateResponse
                => (int)ResponseType.Sensor,

            ServerResponse
                or ServerUpdateResponse
                => (int)ResponseType.Server,

            FactoryResponse
                or FactoryUpdateResponse => (int)ResponseType.Factory,

            PassageResponse => (int)ResponseType.Passage,
            TimeoutResponse => (int)ResponseType.Timeout,
            
            PingResponse => (int)ResponseType.Ping,

            _ => throw new ArgumentException("Tipo de resposta não suportado.", nameof(response))
        };

    private static Dictionary<LiteNet2Commands, Func<TInput, TOutput>> CreateCommandBoardMap<TInput, TOutput>()
    {
        return new Dictionary<LiteNet2Commands, Func<TInput, TOutput>>
        {
            {
                LiteNet2Commands.GetFirmwareVersion,
                board => (TOutput)(object)((LiteNet2Board)(object)board).FirmwareVersion
            },
            { LiteNet2Commands.GetMac, board => (TOutput)(object)((LiteNet2Board)(object)board).Mac },
            { LiteNet2Commands.GetIpMode, board => (TOutput)(object)((LiteNet2Board)(object)board).IpConfig },
            { LiteNet2Commands.GetBuzzerMute, board => (TOutput)(object)((LiteNet2Board)(object)board).BuzzerMute },
            {
                LiteNet2Commands.GetFlowControl,
                board => (TOutput)(object)((LiteNet2Board)(object)board).FirmwareVersion
            },
            {
                LiteNet2Commands.GetFlowControlExtended,
                board => (TOutput)(object)((LiteNet2Board)(object)board).ControlledFlowExtended
            },
            {
                LiteNet2Commands.GetEntryClockwise,
                board => (TOutput)(object)((LiteNet2Board)(object)board).EntryClockwise
            },
            { LiteNet2Commands.GetId, board => (TOutput)(object)((LiteNet2Board)(object)board).Id },
            {
                LiteNet2Commands.GetMessageLine1,
                board => (TOutput)(object)((LiteNet2Board)(object)board).MessageLine1
            },
            {
                LiteNet2Commands.GetMessageLine2,
                board => (TOutput)(object)((LiteNet2Board)(object)board).MessageLine2
            },
            {
                LiteNet2Commands.GetSerialNumber,
                board => (TOutput)(object)((LiteNet2Board)(object)board).SerialNumber
            },
            {
                LiteNet2Commands.GetFingerprintIdentificationMode,
                board => (TOutput)(object)((LiteNet2Board)(object)board).FingerprintIdentificationMode
            },
            {
                LiteNet2Commands.GetShowCounters,
                board => (TOutput)(object)((LiteNet2Board)(object)board).ShowCounters
            },
            {
                LiteNet2Commands.GetReleaseDuration,
                board => (TOutput)(object)((LiteNet2Board)(object)board).ReleaseDuration
            },
            {
                LiteNet2Commands.GetMenuPassword,
                board => (TOutput)(object)((LiteNet2Board)(object)board).MenuPassword
            }
        };
    }

    private static Dictionary<LiteNet2Commands, Func<TInput, TOutput>> CreateCommandResponseMap<TInput, TOutput>(
        string ip, int id, int command, DeviceType type)
    {
        return new Dictionary<LiteNet2Commands, Func<TInput, TOutput>>
        {
            {
                LiteNet2Commands.IdentificationByRfId,
                response => (TOutput)(object)new Notification(ip, id, command, type,
                    GetIdentification((LiteNet2Response)(object)response))
            },
            {
                LiteNet2Commands.IdentificationByBarCode,
                response => (TOutput)(object)new Notification(ip, id, command, type,
                    GetIdentification((LiteNet2Response)(object)response))
            },
            {
                LiteNet2Commands.IdentificationByKeyboard,
                response => (TOutput)(object)new Notification(ip, id, command, type,
                    GetIdentification((LiteNet2Response)(object)response))
            },
            {
                LiteNet2Commands.PositiveIdentificationByFingerprintReader,
                response => (TOutput)(object)new Notification(ip, id, command, type,
                    GetIdentification((LiteNet2Response)(object)response))
            },
            {
                LiteNet2Commands.NegativeIdentificationByFingerprintReader,
                response => (TOutput)(object)new Notification(ip, id, command, type,
                    GetIdentification((LiteNet2Response)(object)response))
            }
        };

        object GetIdentification(LiteNet2Response response)
        {
            return new { response.Identification };
        }
    }
}