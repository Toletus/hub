using System.Text;
using Toletus.Hub.Helpers.SM25Helpers;
using Toletus.Hub.Models;
using Toletus.Hub.Notifications;
using Toletus.Hub.Services.NotificationsServices.Base;
using Toletus.SM25;
using Toletus.SM25.Base;
using Toletus.SM25.Command.Enums;
using SM25Response = Toletus.SM25.Command.SM25Response;

namespace Toletus.Hub.Services.NotificationsServices;

// ReSharper disable once InconsistentNaming
public class SM25NotificationService : NotificationBaseService
{
    protected static Task<Notification> ExecuteCommandAsync(
        Device device,
        SM25Commands command,
        Func<SM25Response?> deviceAction)
    {
        try
        {
            var response = deviceAction();
            var sm25Response = Models.SM25Response.ToResponse(response);

            return Task.FromResult(
                Notification.CreateNotification(
                    device.Ip,
                    device.Id,
                    (int)command,
                    DeviceType.SM25,
                    GetDeviceResponse(sm25Response)));
        }
        catch (FingerprintConnectionException fce)
        {
            return Task.FromResult(
                Notification.CreateNotification(
                    device.Ip,
                    device.Id,
                    (int)command,
                    DeviceType.SM25,
                    new DeviceResponse(false, fce.Message)));
        }
    }

    // ReSharper disable once InconsistentNaming
    protected static void SubscribeSM25Reader(SM25Reader reader)
    {
        reader.OnEnroll += OnEnroll;
        reader.OnEnrollStatus += OnEnrollStatus;
        reader.OnEnrollTimeout += OnEnrollTimeout;
        reader.OnGeneralizationFail += OnGeneralizationFail;
    }

    // ReSharper disable once InconsistentNaming
    protected static void UnsubscribeSM25Reader(SM25Reader? reader)
    {
        if (reader == null) return;

        reader.OnEnroll -= OnEnroll;
        reader.OnEnrollStatus -= OnEnrollStatus;
        reader.OnEnrollTimeout -= OnEnrollTimeout;
        reader.OnGeneralizationFail -= OnGeneralizationFail;
    }

    private static void OnEnroll(int? obj, SM25Reader sm25Reader)
    {
        var description = obj switch
        {
            1 => GdCodeHelper.GetDescription(GDCodes.GD_NEED_FIRST_SWEEP),
            2 => GdCodeHelper.GetDescription(GDCodes.GD_NEED_SECOND_SWEEP),
            3 => GdCodeHelper.GetDescription(GDCodes.GD_NEED_THIRD_SWEEP),
            4 => "Enroll Successful.",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(description)) return;

        var notification = Notification.CreateNotification(
            sm25Reader.Ip.ToString(),
            0,
            (int)SM25Commands.Enroll,
            DeviceType.SM25,
            new { Enroll = description });
        ProcessNotification(sm25Reader.Ip.ToString(), 0, notification: notification);
    }

    private static void OnEnrollStatus(EnrollStatus obj, SM25Reader sm25Reader)
    {
        var description = ReturnCodeHelper.IsSuccess(obj.Ret)
            ? GdCodeHelper.GetDescription(obj.DataGD)
            : ReturnCodeHelper.GetDescription(obj.Ret);

        var notification = Notification.CreateNotification(
            sm25Reader.Ip.ToString(),
            0,
            (int)ResponsePrefixes.ResponseDataPacket,
            DeviceType.SM25,
            new
            {
                EnrollStatus = new
                {
                    IsSuccess = ReturnCodeHelper.IsSuccess(obj.Ret),
                    Description = description
                }
            });
        ProcessNotification(sm25Reader.Ip.ToString(), 0, notification: notification);
    }

    private static void OnEnrollTimeout(SM25Reader sm25Reader)
    {
        var notification = Notification.CreateNotification(
            sm25Reader.Ip.ToString(),
            0,
            (int)ResponsePrefixes.ResponseDataPacket,
            DeviceType.SM25,
            new { EnrollTimeout = true });
        ProcessNotification(sm25Reader.Ip.ToString(), 0, notification: notification);
    }

    private static void OnGeneralizationFail(SM25Reader sm25Reader)
    {
        var notification = Notification.CreateNotification(
            sm25Reader.Ip.ToString(),
            0,
            (int)ResponsePrefixes.ResponseDataPacket,
            DeviceType.SM25,
            new { GeneralizationFail = true });
        ProcessNotification(sm25Reader.Ip.ToString(), 0, notification: notification);
    }

    private static DeviceResponse GetDeviceResponse(Models.SM25Response? sm25Response)
    {
        if (sm25Response is null)
            return Error(DeviceActionNullMessage);

        if (ReturnCodeHelper.IsError(sm25Response.ReturnCode))
            return Error(ReturnCodeHelper.GetDescription(sm25Response.ReturnCode));

        object? content = sm25Response.Command switch
        {
            SM25Commands.GetDeviceName => GetName(sm25Response),

            SM25Commands.GetDeviceID
                or SM25Commands.GetEmptyID
                or SM25Commands.GetEnrollCount
                or SM25Commands.GetFingerTimeOut
                or SM25Commands.GetSecurityLevel
                or SM25Commands.SetDeviceID
                or SM25Commands.SetFingerTimeOut
                or SM25Commands.SetSecurityLevel => sm25Response.Data,

            SM25Commands.GetFWVersion => GetVersion(sm25Response),

            SM25Commands.GetTemplateStatus => TemplateStatusHelper.GetDescription(sm25Response.DataTemplateStatus),

            SM25Commands.GetDuplicationCheck
                or SM25Commands.SetDuplicationCheck => Convert.ToBoolean((int)sm25Response.Data),

            SM25Commands.Enroll
                or SM25Commands.EnrollAndStoreinRAM => GdCodeHelper.GetDescription(sm25Response.DataGD),

            SM25Commands.GetEnrollData => sm25Response.Template,
            SM25Commands.ReadTemplate => sm25Response.RawData,

            SM25Commands.FPCancel
                or SM25Commands.WriteTemplate
                or SM25Commands.ClearTemplate
                or SM25Commands.ClearAllTemplate => sm25Response.ReturnCode == ReturnCodes.ERR_SUCCESS,
            
            _ => sm25Response
        };

        return new DeviceResponse(new { content });
    }

    private static DeviceResponse Error(string msg) => new(false, msg);

    private static string GetName(Models.SM25Response response)
        => Encoding.UTF8.GetString(response.RawData[..^6]);

    private static string GetVersion(Models.SM25Response response)
    {
        var major = (byte)(response.Data & 0xFF);
        var minor = (byte)((response.Data >> 8) & 0xFF);
        return $"v{major}.{minor}";
    }
}