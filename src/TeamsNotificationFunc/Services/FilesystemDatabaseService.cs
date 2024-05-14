using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TeamsNotificationFunc.Services;

public class FilesystemDatabaseService : IDatabaseService
{
    private readonly ILogger _logger;
    private readonly DatabaseOptions _options;

    public FilesystemDatabaseService(ILoggerFactory loggerFactory, IOptions<DatabaseOptions> options)
    {
        _logger = loggerFactory.CreateLogger<FilesystemDatabaseService>();
        _options = options.Value;
    }

    public async Task StoreDocumentAsync(string json)
    {
        if (string.IsNullOrEmpty(_options.Database))
        {
            _logger.LogTrace("Filesystem is not configured.");
            return;
        }

        _logger.LogTrace("Storing notification");

        var name = Path.Combine(_options.Database, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff") + ".json");
        await File.WriteAllTextAsync(name, json);

        _logger.LogTrace("Notification stored");
    }
}
