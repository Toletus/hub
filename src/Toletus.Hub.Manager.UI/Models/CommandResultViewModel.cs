namespace Toletus.Hub.Manager.UI.Models;

public sealed record CommandResultViewModel
{
    public required CommandStatusKind Status { get; init; }
    public required string MessageKey { get; init; }
    public string? TechnicalDetails { get; init; }
    public DeviceRefViewModel? Device { get; init; }
    public object? Data { get; init; }

    public bool IsSuccess => Status == CommandStatusKind.Success;

    public static CommandResultViewModel Success(string messageKey, DeviceRefViewModel? device = null, object? data = null) =>
        new()
        {
            Status = CommandStatusKind.Success,
            MessageKey = messageKey,
            Device = device,
            Data = data
        };

    public static CommandResultViewModel Pending(string messageKey, DeviceRefViewModel? device = null) =>
        new()
        {
            Status = CommandStatusKind.Pending,
            MessageKey = messageKey,
            Device = device
        };

    public static CommandResultViewModel Error(string messageKey, string? technicalDetails = null, DeviceRefViewModel? device = null) =>
        new()
        {
            Status = CommandStatusKind.Error,
            MessageKey = messageKey,
            TechnicalDetails = technicalDetails,
            Device = device
        };

    public static CommandResultViewModel Warning(string messageKey, string? technicalDetails = null, DeviceRefViewModel? device = null) =>
        new()
        {
            Status = CommandStatusKind.Warning,
            MessageKey = messageKey,
            TechnicalDetails = technicalDetails,
            Device = device
        };
}
