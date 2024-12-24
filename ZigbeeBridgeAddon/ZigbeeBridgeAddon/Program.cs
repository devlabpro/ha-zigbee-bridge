using Microsoft.EntityFrameworkCore;
using NetDaemon.AppModel;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Runtime;
using NLog;
using NLog.Config;
using NLog.Targets;
using Radzen;
using System.Collections;
using System.Reflection;
using ZigbeeBridgeAddon.Components;
using ZigbeeBridgeAddon.Data;
using ZigbeeBridgeAddon.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddDbContext<DevicesStore>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DevicesStoreConnectionString"))
    );

    foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
    {
        Console.WriteLine(e.Key + ":" + e.Value);
    }

    var nlogConfig = new LoggingConfiguration();

    nlogConfig.AddRule(minLevel: NLog.LogLevel.Trace, maxLevel: NLog.LogLevel.Fatal,
        target: new ConsoleTarget("consoleTarget")
        {
            Layout = "${longdate} level=${level} message=${message}"
        });

    LogManager.Configuration = nlogConfig;

    builder.Host
        .UseNetDaemonDefaultLogging()
        .UseNetDaemonRuntime()
        .ConfigureServices((_, services) =>
            services
                .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
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
