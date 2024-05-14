
namespace TeamsNotificationFunc.Services;

public interface IDatabaseService
{
    Task StoreDocumentAsync(string json);
}
