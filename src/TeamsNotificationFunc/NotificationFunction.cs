using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TeamsNotificationFunc.Interfaces;
using TeamsNotificationFunc.Services;

public class NotificationFunction
{
    private readonly ILogger _logger;
    private readonly NotificationOptions _options;
    private readonly DecryptionService _decryptionService;

    public NotificationFunction(ILoggerFactory loggerFactory, IOptions<NotificationOptions> options, DecryptionService decryptionService)
    {
        _logger = loggerFactory.CreateLogger<NotificationFunction>();
        _options = options.Value;
        _decryptionService = decryptionService;
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
                var notifications = JsonSerializer.Deserialize<Notifications>(input);
                if (notifications == null)
                {
                    _logger.LogTrace("Notifications null");
                    continue;
                }

                foreach (var notification in notifications.Items)
                {
                    if (notification.SubscriptionId == "NA")
                    {
                        _logger.LogTrace("Skip validation message");
                    }
                    else if (notification.EncryptedContent != null && notification.EncryptedContent.Data != null)
                    {
                        var notificationData = await _decryptionService.DecryptAsync(notification.EncryptedContent);
                    }
                    else
                    {
                        _logger.LogTrace("No processing logic for this message type");
                    }
                }
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
