using Newtonsoft.Json;

namespace TeamsNotificationFunc.Interfaces;

// Taken from sample code:
// https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-change-notification/csharp/ChangeNotification/Model/Notification.cs
public class Notifications
{
    [JsonProperty(PropertyName = "value")]
    public List<Notifications> Items { get; set; } = new();
}

public class Notification
{
    // The type of change.
    [JsonProperty(PropertyName = "changeType")]
    public string ChangeType { get; set; } = string.Empty;

    // The client state used to verify that the notification is from Microsoft Graph. Compare the value received with the notification to the value you sent with the subscription request.
    [JsonProperty(PropertyName = "clientState")]
    public string ClientState { get; set; } = string.Empty;

    // The endpoint of the resource that changed. For example, a message uses the format ../Users/{user-id}/Messages/{message-id}
    [JsonProperty(PropertyName = "resource")]
    public string Resource { get; set; } = string.Empty;

    // The UTC date and time when the webhooks subscription expires.
    [JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
    public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

    // The unique identifier for the webhooks subscription.
    [JsonProperty(PropertyName = "subscriptionId")]
    public string SubscriptionId { get; set; } = string.Empty;

    // Properties of the changed resource.
    [JsonProperty(PropertyName = "resourceData")]
    public ResourceData? ResourceData { get; set; }
}

// Taken from sample code:
// https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-meeting-notification/csharp/MeetingNotification/Model/ResourceData.cs
public class ResourceData
{
    // The ID of the resource.
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    // The OData etag property.
    [JsonProperty(PropertyName = "@odata.type")]
    public string ODataEType { get; set; } = string.Empty;

    // The OData ID of the resource. This is the same value as the resource property.
    [JsonProperty(PropertyName = "@odata.id")]
    public string ODataId { get; set; } = string.Empty;

    // The OData type of the resource: "#Microsoft.Graph.Message", "#Microsoft.Graph.Event", or "#Microsoft.Graph.Contact".
    [JsonProperty(PropertyName = "activity")]
    public string Activity { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "availability")]
    public string Availability { get; set; } = string.Empty;
}
