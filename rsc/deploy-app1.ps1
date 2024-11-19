# https://learn.microsoft.com/en-us/powershell/microsoftgraph/installation?view=graph-powershell-1.0
# https://learn.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent
Install-Module Microsoft.Graph.Beta -Repository PSGallery -Scope CurrentUser -Force

Connect-MgGraph -Scopes @('TeamworkAppSettings.ReadWrite.All', 'Policy.ReadWrite.Authorization', 'AppCatalog.Read.All', 'Policy.ReadWrite.PermissionGrant', 'InformationProtectionPolicy.Read', 'Application.ReadWrite.All')

# https://learn.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/preapproval-instruction-docs
Get-MgBetaTeamRscConfiguration
# Id                                          State              ScopeType
# --                                          -----              ---------
# TeamResourceSpecificPermissionConfiguration ManagedByMicrosoft Team

Get-MgBetaChatRscConfiguration
# Id                                          State             ScopeType
# --                                          -----             ---------
# ChatResourceSpecificPermissionConfiguration EnabledForAllApps Chat

Set-MgBetaTeamRscConfiguration -State EnabledForAllApps
# Set-MgBetaTeamRscConfiguration -State ManagedByMicrosoft
Set-MgBetaChatRscConfiguration -State EnabledForAllApps

# Install app: "RSC Demo App1" (under "rsc-demo-app1" folder)
# App reg must have: "signInAudience": "AzureADMultipleOrgs"!

# **Alternatively**, you can enable the resource-specific consent for a specific app
# Get the app ID:
# https://graph.microsoft.com/beta/appCatalogs/teamsApps
$permissions = "ChannelMessage.Read.Group", "ChannelMessage.Send.Group", "ChannelSettings.Read.Group", "TeamMember.Read.Group", "Owner.Read.Group", "Member.Read.Group"
New-MgBetaTeamAppPreapproval -TeamsAppId "2037dd85-99c6-4004-a013-72de858777b9" -ResourceSpecificApplicationPermissionsAllowedForTeams $permissions -TeamLevelSensitivityLabelCondition AnySensitivityLabel

# Verify installation:
# https://admin.teams.microsoft.com/policies/manage-apps

$clientId = "<put your client id here>"
$clientSecret = "<put your client secret here>"
$tenantId = "<put your tenant id here>"
$teamId = "<put your team id here>"
$channelId = "<put your channel id here>"

$eventHubNamespace = "<put your event hub namespace here>"
$eventHubName = "<put your event hub name here>"

# Check permission grants from specific team & channel using e.g., Graph Explorer
# https://learn.microsoft.com/en-us/graph/api/group-list-permissiongrants?view=graph-rest-1.0&tabs=http
Start-Process "https://developer.microsoft.com/en-us/graph/graph-explorer"
"https://graph.microsoft.com/beta/teams/$teamId/permissionGrants" | Set-Clipboard
# Missing scope permissions on the request. 
# API requires one of 'ResourceSpecificPermissionGrant.ReadForTeam, 
# TeamsAppInstallation.ReadForTeam, TeamsAppInstallation.ReadWriteSelfForTeam, 
# TeamsAppInstallation.ReadWriteAndConsentSelfForTeam, TeamsAppInstallation.ReadWriteForTeam, 
# TeamsAppInstallation.ReadWriteAndConsentForTeam, Group.Read.All, Directory.Read.All, 
# Group.ReadWrite.All, Directory.ReadWrite.All'. 
# Scopes on the request '...'"
# ->
# {
#   "@odata.context": "https://graph.microsoft.com/beta/$metadata#permissionGrants",
#   "@microsoft.graph.tips": "Use $select to choose only the properties your app needs, as this can lead to performance improvements. For example: GET teams('<guid>')/permissionGrants?$select=clientAppId,clientId",
#   "value": [
#   {
#       "id": "aFHfWJmOhzjufOjGo_lLh9h2vIjEqUhxF6A55Dv75ys",
#       "clientAppId": "f0f8260d-cd5c-438a-a708-9872406fc67b",
#       "resourceAppId": "00000003-0000-0000-c000-000000000000",
#       "clientId": "f704ba19-9e41-4f49-af57-deac7f1b583c",
#       "permissionType": "Application",
#       "permission": "ChannelMessage.Read.Group"
#   },
#   {
#       "id": "hUACblG_yriO5urrYB2HquSVwdFJ1Snzn1RXbsl39wc",
#       "clientAppId": "f0f8260d-cd5c-438a-a708-9872406fc67b",
#       "resourceAppId": "00000003-0000-0000-c000-000000000000",
#       "clientId": "f704ba19-9e41-4f49-af57-deac7f1b583c",
#       "permissionType": "Application",
#       "permission": "ChannelMessage.Send.Group"
#   },
#   {
#       "id": "Ekm59ztftzTudB2ASxWULeqWeNpMFUX16qpTJfW152c",
#       "clientAppId": "f0f8260d-cd5c-438a-a708-9872406fc67b",
#       "resourceAppId": "00000003-0000-0000-c000-000000000000",
#       "clientId": "f704ba19-9e41-4f49-af57-deac7f1b583c",
#       "permissionType": "Application",
#       "permission": "ChannelSettings.Read.Group"
#   },
#   {
#       "id": "ereBPdf_VO95J7LVITHPQgA6hoZBuk-cldo78fqChYI",
#       "clientAppId": "f0f8260d-cd5c-438a-a708-9872406fc67b",
#       "resourceAppId": "00000003-0000-0000-c000-000000000000",
#       "clientId": "f704ba19-9e41-4f49-af57-deac7f1b583c",
#       "permissionType": "Application",
#       "permission": "Member.Read.Group"
#   },
#   {
#       "id": "9GqPb1WR61zkH-fJDgjxbae6MXxBtf7kg_ojIOALckY",
#       "clientAppId": "f0f8260d-cd5c-438a-a708-9872406fc67b",
#       "resourceAppId": "00000003-0000-0000-c000-000000000000",
#       "clientId": "f704ba19-9e41-4f49-af57-deac7f1b583c",
#       "permissionType": "Application",
#       "permission": "Owner.Read.Group"
#   },
#   {
#       "id": "iUCYf_-xLPVT98QHAurkKv5iSnUWOqid9seRN6ucty0",
#       "clientAppId": "f0f8260d-cd5c-438a-a708-9872406fc67b",
#       "resourceAppId": "00000003-0000-0000-c000-000000000000",
#       "clientId": "f704ba19-9e41-4f49-af57-deac7f1b583c",
#       "permissionType": "Application",
#       "permission": "TeamMember.Read.Group"
#   }
#   ]
# }

# ------------------------------
# Switch to service principal
# ------------------------------

$clientPassword = ConvertTo-SecureString $clientSecret -AsPlainText -Force
$credentials = New-Object System.Management.Automation.PSCredential($clientID, $clientPassword)
Connect-AzAccount -ServicePrincipal -Credential $credentials -TenantId $tenantId

(Get-AzAccessToken -ResourceUrl "https://graph.microsoft.com").Token | Set-Clipboard
# "roles": [
#    "Group.Selected"
#  ],

Invoke-AzRestMethod `
    -Uri "https://graph.microsoft.com/v1.0/teams/$teamId"
# Missing role permissions on the request. API requires one of 'Team.ReadBasic.All, 
# TeamSettings.Read.All, TeamSettings.ReadWrite.All, Group.Read.All, Directory.Read.All, 
# Group.ReadWrite.All, Directory.ReadWrite.All, TeamSettings.Read.Group, TeamSettings.Edit.Group, 
# TeamSettings.ReadWrite.Group'. Roles on the request 'Group.Selected'. 
# Resource specific consent grants on the request 'ChannelMessage.Read.Group, 
# ChannelMessage.Send.Group, ChannelSettings.Read.Group, Member.Read.Group, Owner.Read.Group, TeamMember.Read.Group'.

Invoke-AzRestMethod `
    -Uri "https://graph.microsoft.com/v1.0/teams/$teamId/channels"

Invoke-AzRestMethod `
    -Uri "https://graph.microsoft.com/v1.0/teams/$teamId/channels/$channelId"

Invoke-AzRestMethod `
    -Uri "https://graph.microsoft.com/v1.0/teams/$teamId/channels/$channelId/messages"

Invoke-AzRestMethod `
    -Uri "https://graph.microsoft.com/v1.0/teams/$teamId/channels/$channelId/messages" `
| findstr Hello

# Subscriptions
Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/subscriptions"

# Required:
# "Azure Event Hubs Data Sender" role for "Microsoft Graph Change Tracking"!
$notificationUrl = "EventHub:https://$eventHubNamespace.servicebus.windows.net/eventhubname/$eventHubName`?tenantId=$tenantId"
$clientState = [guid]::NewGuid().ToString()
$body = @{
    changeType                = "created,updated"
    notificationUrl           = $notificationUrl
    lifecycleNotificationUrl  = $notificationUrl
    resource                  = "/teams/$teamId/channels/$channelId/messages"
    expirationDateTime        = (Get-Date).AddDays(3).ToUniversalTime()
    clientState               = $clientState
    latestSupportedTlsVersion = "v1_2"
    includeResourceData       = $false
} | ConvertTo-Json
$body

$subscriptionResponse = Invoke-AzRestMethod -Uri "https://graph.microsoft.com/v1.0/subscriptions" -Method Post -Payload $body
$subscriptionContent = $subscriptionResponse.Content | ConvertFrom-Json
$subscriptionContent
