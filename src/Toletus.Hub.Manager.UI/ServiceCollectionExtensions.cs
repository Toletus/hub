using Microsoft.Extensions.DependencyInjection;
using Toletus.Hub.Manager.UI.Capabilities;
using Toletus.Hub.Manager.UI.Services;

namespace Toletus.Hub.Manager.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubManagerUi(this IServiceCollection services)
    {
        services.AddSingleton<CultureState>();
        services.AddSingleton<UiText>();
        services.AddSingleton<DeviceCapabilityCatalog>();
        services.AddSingleton<NotificationHistoryService>();
        services.AddScoped<ManagerUiState>();

        return services;
    }
}
