using Toletus.Hub.Services;

namespace Toletus.Hub;

/// <summary>
/// Provides methods to configure the dependency injection container for the application.
/// </summary>
public static class BootStrapper
{
    /// <summary>
    /// Registers services with the dependency injection container within the provided WebApplicationBuilder instance.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder instance to which services will be added.</param>
    public static void RegisterIoC(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddSingleton<ControllerService>();
        services.AddSingleton<DeviceService>();
        services.AddSingleton<BasicCommonCommandService>();
        services.AddSingleton<LiteNet1CommandService>();
        services.AddSingleton<LiteNet2CommandService>();
        services.AddSingleton<LiteNet3CommandService>();
        services.AddSingleton<SM25ReaderCommandsService>();
    }
}