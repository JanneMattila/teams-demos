using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TeamsNotificationFunc.Services;

public class DatabaseService
{
    private readonly ILogger _logger;
    private readonly DatabaseOptions _options;
    private Container? _container;

    public DatabaseService(ILoggerFactory loggerFactory, IOptions<DatabaseOptions> options)
    {
        _logger = loggerFactory.CreateLogger<DatabaseService>();
        _options = options.Value;
    }

    public async Task StoreDocumentAsync(string json)
    {
        if (string.IsNullOrEmpty(_options.CosmosUrl))
        {
            _logger.LogTrace("Cosmos is not configured.");
            return;
        }

        if (_container == null)
        {
            var client = new CosmosClient(_options.CosmosUrl, new DefaultAzureCredential());
            var database = client.GetDatabase(_options.Database);
            _container = database.GetContainer(_options.Container);
        }

        await _container.UpsertItemAsync(json);
    }
}
