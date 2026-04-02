param(
    [Parameter(Mandatory=$true)]
    [string]$SubscriptionId,

    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,

    [Parameter(Mandatory=$true)]
    [string]$FunctionAppName,

    [Parameter(Mandatory=$false)]
    [string]$LocalSettingsPath = "local.settings.json"
)

# Set the subscription context
Write-Host "Setting subscription context to $SubscriptionId..."
az account set --subscription $SubscriptionId

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to set subscription context"
    exit 1
}

# Check if local.settings.json exists
if (-not (Test-Path $LocalSettingsPath)) {
    Write-Error "local.settings.json not found at path: $LocalSettingsPath"
    exit 1
}

# Read the local.settings.json file
Write-Host "Reading local.settings.json..."
$settings = Get-Content $LocalSettingsPath | ConvertFrom-Json

if ($null -eq $settings.Values) {
    Write-Error "No 'Values' object found in local.settings.json"
    exit 1
}

# Build the settings string for Azure CLI command
$appSettings = @()

# Settings to exclude from deployment
$excludedSettings = @("FUNCTIONS_WORKER_RUNTIME", "AzureWebJobsStorage")

# Add top-level Values
if ($null -ne $settings.Values) {
    foreach ($key in $settings.Values.PSObject.Properties.Name) {
        # Skip excluded settings
        if ($excludedSettings -contains $key) {
            Write-Host "Skipping excluded setting: $key"
            continue
        }
        $value = $settings.Values.$key
        $appSettings += "$key=$value"
    }
}

# Add nested configuration objects using __ separator
$nestedObjects = @("ApprenticeshipApi", "CosmosDb", "WhatsApp")
foreach ($objectName in $nestedObjects) {
    if ($null -ne $settings.$objectName) {
        foreach ($key in $settings.$objectName.PSObject.Properties.Name) {
            $value = $settings.$objectName.$key
            $appSettings += "$objectName`__$key=$value"
        }
    }
}

# Update the Function App settings
Write-Host "Updating Function App settings for $FunctionAppName in $ResourceGroupName..."
az functionapp config appsettings set `
    --name $FunctionAppName `
    --resource-group $ResourceGroupName `
    --settings $appSettings

if ($LASTEXITCODE -eq 0) {
    Write-Host "Successfully updated Function App settings"
    Write-Host "Updated settings:"
    $appSettings | ForEach-Object { Write-Host "  $_" }
} else {
    Write-Error "Failed to update Function App settings"
    exit 1
}
