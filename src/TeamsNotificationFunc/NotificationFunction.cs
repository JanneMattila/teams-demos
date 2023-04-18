using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AzureDigitalTwinsUpdaterFunc;

public class NotificationFunction
{
    private readonly ILogger _logger;
    private readonly NotificationOptions _options;

    public NotificationFunction(ILoggerFactory loggerFactory, IOptions<NotificationOptions> options)
    {
        _logger = loggerFactory.CreateLogger<NotificationFunction>();
        _options = options.Value;
    }

    [Function("NotificationFunc")]
    public async Task Run([EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString")] string[] inputs)
    {
        var exceptions = new List<Exception>();
        foreach (var input in inputs)
        {
            try
            {
                _logger.LogTrace("Function processing message: {Input}", input);
                var notification = JsonSerializer.Deserialize<Dictionary<string, object>>(input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while processing message");
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }

        if (exceptions.Count == 1)
        {
            throw exceptions.Single();
        }

        await Task.CompletedTask;
    }
}
