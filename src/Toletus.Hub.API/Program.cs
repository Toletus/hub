using Scalar.AspNetCore;
using Toletus.Hub;
using Toletus.Hub.Services;
using Toletus.Hub.Services.NotificationsServices;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);

var app = builder.Build();

ConfigureMiddlewares(app);

app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    InitializeNotificationServices();
    builder.RegisterIoC();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
}

void ConfigureMiddlewares(WebApplication app)
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.Lifetime.ApplicationStopping.Register(ApplicationStopping);
    app.MapControllers();
}

void ApplicationStopping()
{
    Console.WriteLine("The application is shutting down...");
}

void InitializeNotificationServices()
{
    LiteNet1NotificationService.Initialize();
    LiteNet2NotificationService.Initialize();
    LiteNet3NotificationService.Initialize();
}