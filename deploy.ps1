# https://learn.microsoft.com/en-us/graph/change-notifications-overview
$keyVaultName = "kvname"
$secretName = "secretname"

$storageName = "storagename"

$eventHubNamespace = "eventhubnamespace"
$eventHubName = "eventhubname"
$tenant = "contoso.com"

# Certificate password
$certificatePasswordPlainText = "<your certificate password>"
$certificatePassword = ConvertTo-SecureString -String $certificatePasswordPlainText -Force -AsPlainText

$certAdminTool = New-SelfSignedCertificate -certstorelocation cert:\currentuser\my -subject "CN=TeamsAdminTool"

# Export pfx
Export-PfxCertificate -Cert $certAdminTool -FilePath teamsadmintool.pfx -Password $certificatePassword

# Export cer
Export-Certificate -Cert $certAdminTool -FilePath teamsadmintool.cer

# Set thumbprint variables for later use
$admintoolThumbprint = $certAdminTool.thumbprint

########################
# Login as "AdminTool"
########################

Connect-AzAccount -ServicePrincipal -ApplicationId $admintoolClientId -Tenant $tenantId -CertificateThumbprint $admintoolThumbprint

# https://learn.microsoft.com/en-us/graph/teams-changenotifications-chatmessage#subscribe-to-messages-in-a-channel
# 

# Certificate password
$certificatePasswordPlainText = "<your certificate password>"
$certificatePassword = ConvertTo-SecureString -String $certificatePasswordPlainText -Force -AsPlainText

$certTeamsTool = New-SelfSignedCertificate -certstorelocation cert:\currentuser\my -subject "CN=TeamsTool"

# Export pfx
Export-PfxCertificate -Cert $certTeamsTool -FilePath teamstool.pfx -Password $certificatePassword

# Export cer
Export-Certificate -Cert $certTeamsTool -FilePath teamstool.cer

$encryptionCertificate = [Convert]::ToBase64String([IO.File]::ReadAllBytes("teamstool.cer"))

# Set thumbprint variables for later use
$teamsToolThumbprint = $certTeamsTool.thumbprint

(Get-AzAccessToken -ResourceUrl "https://graph.microsoft.com").Token | clip
# https://jwt.ms
# This is ***REQUIRED*** ->
# "roles": [
#    "ChannelSettings.Read.All",
#    "ChannelMessage.Read.All",
#    "TeamSettings.Read.All"
# ]

# Get configured change notification subscriptions
Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/subscriptions"
# Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/subscriptions/6ee849d8-df15-450e-b6c4-204b4f3a1b5b" -Method DELETE

$notificationUrl = "EventHub:https://$eventHubNamespace.servicebus.windows.net/eventhubname/$eventHubName`?tenantId=$tenant"
$notificationUrl

$blobStoreUrl = "https://$keyVaultName.vault.azure.net/secrets/$secretName`?tenantId=$tenant"
$blobStoreUrl

$teamsResponse = Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/teams"
$teams = $teamsResponse.Content | ConvertFrom-Json
$team = $teams.value[0] # Get the first one
$team

$channelsResponse = Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/teams/$($team.id)/channels"
$channels = $channelsResponse.Content | ConvertFrom-Json
$channel = $channels.value[0] # Get the first one
$channel

# Create a new subscription
# https://learn.microsoft.com/en-us/graph/api/subscription-post-subscriptions?view=graph-rest-1.0&tabs=http
# https://learn.microsoft.com/en-us/graph/change-notifications-with-resource-data?tabs=csharp
$clientState = [guid]::NewGuid().ToString()
$body = @{
    changeType                = "created,updated"
    notificationUrl           = $notificationUrl
    lifecycleNotificationUrl  = $notificationUrl
    blobStoreUrl              = $blobStoreUrl
    resource                  = "/teams/$($team.id)/channels/$($channel.id)/messages"
    expirationDateTime        = (Get-Date).AddDays(3).ToUniversalTime()
    clientState               = $clientState
    latestSupportedTlsVersion = "v1_2"
    includeResourceData       = $true
    encryptionCertificate     = $encryptionCertificate
    encryptionCertificateId   = "teams-encryption-1"
} | ConvertTo-Json
$body

$subscriptionResponse = Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/subscriptions" -Method Post -Payload $body
$subscriptionContent = $subscriptionResponse.Content | ConvertFrom-Json

# https://learn.microsoft.com/en-us/graph/change-notifications-overview#subscription-lifetime
#
# "Subscription expiration can only be 10080 minutes in the future."
# "Operation: Create; Exception: [Status Code: BadRequest; Reason: Expiration time '2024-05-19T12:16:55.8224757+00:00' 
#  ('-5.23:59:55.5782571' from now) is beyond allowed max '3.00:00:00'.]"

# Reauthorize
# https://learn.microsoft.com/en-us/graph/api/subscription-reauthorize?view=graph-rest-1.0&tabs=http

$reauthorizeBody = @{
    expirationDateTime = (Get-Date).AddDays(3).ToUniversalTime()
} | ConvertTo-Json
$reauthorizeBody

$reauthorizeResponse = Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/subscriptions/$($subscriptionContent.id)" -Method Patch -Payload $reauthorizeBody
$reauthorizeResponse
