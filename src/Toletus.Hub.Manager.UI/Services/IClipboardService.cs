namespace Toletus.Hub.Manager.UI.Services;

public interface IClipboardService
{
    Task<bool> CopyTextAsync(string text, CancellationToken cancellationToken = default);
}
