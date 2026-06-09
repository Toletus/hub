using SixLabors.ImageSharp.Formats.Png;
using Toletus.Hub.Manager.UI.Models;
using Toletus.Hub.Manager.UI.Services;
using Toletus.LiteNet3.Handler.Biometrics.Images;

namespace Toletus.Hub.Manager.Maui.Services;

public sealed class HubCommandHistoryFormatter : CommandHistoryFormatter
{
    public override CommandHistoryItemViewModel Format(
        DateTimeOffset timestamp,
        string commandId,
        CommandResultViewModel result)
    {
        var item = base.Format(timestamp, commandId, result);
        var allowDirectBytes = commandId.Contains("biometrics", StringComparison.OrdinalIgnoreCase);
        if (!TryFindByteArray(result.Data, "Biometrics", allowDirectBytes, out var biometrics))
            return item;

        var details = item.Details
            .Where(detail => !detail.LabelKey.Contains("Biometrics", StringComparison.OrdinalIgnoreCase))
            .ToList();

        details.Insert(0, new HistoryDetailViewModel("History.Detail.BiometricsSize", biometrics.Length.ToString()));

        var media = TryCreateImageMedia(biometrics) ?? TryCreateRawBiometricsPngMedia(biometrics);
        if (media is null)
            details.Insert(1, new HistoryDetailViewModel(
                "History.Detail.BiometricsFormat",
                "unknown",
                "History.Value.ImageFormatUnknown"));

        return item with
        {
            Details = details,
            Media = media
        };
    }

    private static bool TryFindByteArray(object? value, string propertyName, bool allowDirectBytes, out byte[] bytes)
    {
        bytes = [];
        if (value is null)
            return false;

        if (allowDirectBytes && value is byte[] directBytes)
        {
            bytes = directBytes;
            return true;
        }

        var property = value.GetType().GetProperty(propertyName);
        if (property?.GetValue(value) is byte[] propertyBytes)
        {
            bytes = propertyBytes;
            return true;
        }

        var dataProperty = value.GetType().GetProperty("Data");
        if (dataProperty?.GetValue(value) is { } dataValue)
            return TryFindByteArray(dataValue, propertyName, allowDirectBytes, out bytes);

        var responseProperty = value.GetType().GetProperty("Response");
        if (responseProperty?.GetValue(value) is { } responseValue)
            return TryFindByteArray(responseValue, propertyName, allowDirectBytes, out bytes);

        return false;
    }

    private static HistoryMediaViewModel TryCreateRawBiometricsPngMedia(byte[] payload)
    {
        var imageProcessor = new ImageProcessor();
        var image = imageProcessor.CreateImageFromData(payload);
        
        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        var byteArray = ms.ToArray();

        return new HistoryMediaViewModel(
            "image",
            $"data:image/png;base64,{Convert.ToBase64String(byteArray)}",
            "History.Media.BiometricsAlt",
            "History.Media.BiometricsCaption");
    }
}