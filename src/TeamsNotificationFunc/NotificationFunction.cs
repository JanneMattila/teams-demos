using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TeamsNotificationFunc.Interfaces;
using TeamsNotificationFunc.Services;

public class NotificationFunction
{
    private readonly ILogger _logger;
    private readonly DecryptionService _decryptionService;
    private readonly IDatabaseService _databaseService;

    public NotificationFunction(ILoggerFactory loggerFactory, DecryptionService decryptionService, IDatabaseService databaseService)
    {
        _logger = loggerFactory.CreateLogger<NotificationFunction>();
        _decryptionService = decryptionService;
        _databaseService = databaseService;
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
                        await _databaseService.StoreDocumentAsync(notificationData);
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
