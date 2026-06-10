using Microsoft.Extensions.DependencyInjection;
using Toletus.Hub.Manager.UI.Capabilities;
using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Services;

namespace Toletus.Hub.Manager.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubManagerUi(this IServiceCollection services)
    {
        services.AddSingleton<CultureState>();
        services.AddSingleton<UiText>();
        services.AddSingleton<DeviceCapabilityCatalog>();
        services.AddSingleton<ICommandHistoryFormatter, CommandHistoryFormatter>();
        services.AddSingleton<DeviceDisplayStatusService>();
        services.AddSingleton<TimelinePresentationMapper>();
        services.AddSingleton<NotificationHistoryService>();
        services.AddScoped<HubManagerThemeState>();
        services.AddScoped<ManagerUiState>();

        return services;
    }
}
