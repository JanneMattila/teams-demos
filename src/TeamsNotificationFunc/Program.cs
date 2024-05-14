using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamsNotificationFunc.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(s =>
    {
        s.AddApplicationInsightsTelemetryWorkerService();
        s.AddLogging(builder =>
        {
            builder.AddApplicationInsights();
        });

        s.AddOptions<NotificationOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("NotificationOptions").Bind(settings);
            });
        s.AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("DatabaseOptions").Bind(settings);
            });

        s.AddSingleton<DecryptionService>();

        // Use Cosmos DB for backend storage:
        //s.AddSingleton<IDatabaseService, DatabaseService>();

        // Use local filesystem for backend storage:
        s.AddSingleton<IDatabaseService, FilesystemDatabaseService>();
    })
    .Build();

host.Run();
