using System.Text.Json.Serialization;

namespace TeamsNotificationFunc.Interfaces;

// Taken from sample code:
// https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-change-notification/csharp/ChangeNotification/Model/Notification.cs
public class Notifications
{
    [JsonPropertyName("value")]
    public List<Notification> Items { get; set; } = new();
}

public class Notification
{
    // The type of change.
    [JsonPropertyName("changeType")]
    public string ChangeType { get; set; } = string.Empty;

    // The client state used to verify that the notification is from Microsoft Graph. Compare the value received with the notification to the value you sent with the subscription request.
    [JsonPropertyName("clientState")]
    public string ClientState { get; set; } = string.Empty;

    // The endpoint of the resource that changed. For example, a message uses the format ../Users/{user-id}/Messages/{message-id}
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;

    // The UTC date and time when the webhooks subscription expires.
    [JsonPropertyName("subscriptionExpirationDateTime")]
    public string SubscriptionExpirationDateTime { get; set; }

    // The unique identifier for the webhooks subscription.
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; set; } = string.Empty;

    // The tenant identifier.
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;

    // Properties of the changed resource.
    [JsonPropertyName("resourceData")]
    public ResourceData? ResourceData { get; set; }

    [JsonPropertyName("encryptedContent")]
    public EncryptedContentData? EncryptedContent { get; set; }
}

// Taken from sample code:
// https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-meeting-notification/csharp/MeetingNotification/Model/ResourceData.cs
public class ResourceData
{
    // The ID of the resource.
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    // The OData etag property.
    [JsonPropertyName("@odata.type")]
    public string ODataEType { get; set; } = string.Empty;

    // The OData ID of the resource. This is the same value as the resource property.
    [JsonPropertyName("@odata.id")]
    public string ODataId { get; set; } = string.Empty;

    // The OData type of the resource: "#Microsoft.Graph.Message", "#Microsoft.Graph.Event", or "#Microsoft.Graph.Contact".
    [JsonPropertyName("activity")]
    public string Activity { get; set; } = string.Empty;

    [JsonPropertyName("availability")]
    public string Availability { get; set; } = string.Empty;
}

// Taken from sample code:
// https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/graph-meeting-notification/csharp/MeetingNotification/Model/Encryptedcontent.cs
public class EncryptedContentData
{
    /// <summary>
    /// Encrypted data.
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted data signature.
    /// </summary>
    [JsonPropertyName("dataSignature")]
    public string DataSignature { get; set; } = string.Empty;

    /// <summary>
    /// Data key of encrypted content.
    /// </summary>
    [JsonPropertyName("dataKey")]
    public string DataKey { get; set; } = string.Empty;

    /// <summary>
    /// Certificate key.
    /// </summary>
    [JsonPropertyName("encryptionCertificateId")]
    public string EncryptionCertificateId { get; set; } = string.Empty;

    /// <summary>
    /// Certificate thumbprint.
    /// </summary>
    [JsonPropertyName("encryptionCertificateThumbprint")]
    public string EncryptionCertificateThumbprint { get; set; } = string.Empty;
}
