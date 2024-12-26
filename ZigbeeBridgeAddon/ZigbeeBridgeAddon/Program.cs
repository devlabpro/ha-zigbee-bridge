using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using NetDaemon.Client.Settings;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Runtime;
using NLog;
using NLog.Config;
using NLog.Targets;
using Radzen;
using System.Collections;
using ZigbeeBridgeAddon.Components;
using ZigbeeBridgeAddon.Data;
using ZigbeeBridgeAddon.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
    foreach (var path in Directory.GetFiles(@"/data"))
    {
        Console.WriteLine(path); // full path
        Console.WriteLine(System.IO.Path.GetFileName(path)); // file name
        string[] fileLines = System.IO.File.ReadAllLines(path);
        if (fileLines != null)
            foreach (var line in fileLines)
            {
                Console.WriteLine(line);
            }
    }
    foreach (var path in Directory.GetDirectories(@"/"))
    {
        Console.WriteLine(path); // full path
    }

    builder.Services.AddDbContext<DevicesStore>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DevicesStoreConnectionString"))
    );

    var token = Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN");
    var haSettings = new HomeAssistantSettings();
    if (!string.IsNullOrEmpty(token))
    {
        haSettings.Token = token;
        haSettings.Host = "supervisor";
        haSettings.WebsocketPath = "core/websocket";
        haSettings.Port = 80;
    }

    var nlogConfig = new LoggingConfiguration();

    nlogConfig.AddRule(minLevel: NLog.LogLevel.Trace, maxLevel: NLog.LogLevel.Fatal,
        target: new ConsoleTarget("consoleTarget")
        {
            Layout = "${longdate} level=${level} message=${message}"
        });

    LogManager.Configuration = nlogConfig;

    builder.Host
        .UseNetDaemonAppSettings()
        .UseNetDaemonDefaultLogging()
        .UseNetDaemonRuntime()
        .ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "HomeAssistant:Host", haSettings.Host },
                { "HomeAssistant:Token", haSettings.Token },
                { "HomeAssistant:WebsocketPath", haSettings.WebsocketPath },
                { "HomeAssistant:Port", haSettings.Port.ToString() }
            });
        })
        .ConfigureServices((_, services) =>
            services
                .AddNetDaemonStateManager()
                .AddNetDaemonScheduler()
        );

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddRadzenComponents();

    builder.Services.AddSingleton<SerialClientService>();
    builder.Services.AddScoped<HAService>();
    builder.Services.AddScoped<DBService>();
    builder.Services.AddHostedService<BackgroundWorker>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DevicesStore>();
        if (db.Database.GetPendingMigrations().Any())
        {
            db.Database.Migrate();
        }
    }


    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception e)
{
    Console.WriteLine($"Failed to start host... {e}");
throw;
}
