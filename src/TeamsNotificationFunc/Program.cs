using AzureDigitalTwinsUpdaterFunc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
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
    })
    .Build();

host.Run();
