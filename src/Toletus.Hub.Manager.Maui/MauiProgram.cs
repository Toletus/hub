using Microsoft.Extensions.Logging;
using Toletus.Hub.Manager.Maui.Services;
using Toletus.Hub.Manager.UI;
using Toletus.Hub.Manager.UI.Contracts;
using Toletus.Hub.Manager.UI.Services;
using Toletus.Hub.Services;
using Toletus.Hub.Services.NotificationsServices;

namespace Toletus.Hub.Manager.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        InitializeNotificationServices();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddHubManagerUi();
        builder.Services.AddSingleton<ICommandHistoryFormatter, HubCommandHistoryFormatter>();
        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddSingleton<ControllerService>();
        builder.Services.AddSingleton<BasicCommonCommandService>();
        builder.Services.AddSingleton<LiteNet1CommandService>();
        builder.Services.AddSingleton<LiteNet2CommandService>();
        builder.Services.AddSingleton<LiteNet3CommandService>();
        builder.Services.AddSingleton<SM25ReaderCommandsService>();
        builder.Services.AddSingleton<IHubDeviceManager, HubDirectDeviceManager>();
        builder.Services.AddSingleton<HubNotificationHistoryBridge>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        _ = app.Services.GetRequiredService<HubNotificationHistoryBridge>();
        return app;
    }

    private static void InitializeNotificationServices()
    {
        LiteNet1NotificationService.Initialize();
        LiteNet2NotificationService.Initialize();
        LiteNet3NotificationService.Initialize();
    }
}
