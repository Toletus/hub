using System.Resources;

namespace Toletus.Hub.Manager.UI.Services;

public sealed class UiText(CultureState cultureState)
{
    private static readonly ResourceManager ResourceManager =
        new("Toletus.Hub.Manager.UI.Resources.SharedResource", typeof(UiText).Assembly);

    public string this[string key] => Get(key);

    public string Get(string key) =>
        ResourceManager.GetString(key, cultureState.CurrentCulture) ?? key;
}
