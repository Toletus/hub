using System.Globalization;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class CultureState
{
    public event Action? Changed;

    public CultureInfo CurrentCulture { get; private set; } = new("pt-BR");

    public IReadOnlyList<CultureInfo> SupportedCultures { get; } =
    [
        new("pt-BR"),
        new("en-US")
    ];

    public void SetCulture(string cultureName)
    {
        var culture = SupportedCultures.FirstOrDefault(c => c.Name.Equals(cultureName, StringComparison.OrdinalIgnoreCase));
        if (culture is null || culture.Name == CurrentCulture.Name)
            return;

        CurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Changed?.Invoke();
    }
}
