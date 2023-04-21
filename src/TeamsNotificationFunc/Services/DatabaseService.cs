using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TeamsNotificationFunc.Data;

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

        _logger.LogTrace("Storing notification");

        if (_container == null)
        {
            var serializerOptions = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var serializer = new CosmosSystemTextJsonSerializer(serializerOptions);
            var clientOptions = new CosmosClientOptions() { Serializer = serializer };
            var client = new CosmosClient(_options.CosmosUrl, new DefaultAzureCredential(), clientOptions);
            var database = client.GetDatabase(_options.Database);
            _container = database.GetContainer(_options.Container);
        }

        var data = JsonObject.Parse(json);

        // You can override the id here if you want to
        // data["id"] = Guid.NewGuid().ToString();

        await _container.CreateItemAsync(data);
        _logger.LogTrace("Notification stored");
    }
}
