param location string = resourceGroup().location
param appName string = 'uk-apprenticeships'
@secure()
param apprenticeshipApiKey string
@secure()
param whatsAppAccessToken string
param whatsAppPhoneNumberId string = '1068222259699217'
param whatsAppTemplateName string = 'new_vacancy'

var uniqueSuffix = uniqueString(resourceGroup().id)
var storageAccountName = 'st${take(replace(appName, '-', ''), 11)}${take(uniqueSuffix, 11)}'
var functionAppName = '${appName}-func-${uniqueSuffix}'
var cosmosAccountName = '${appName}-cosmos-${uniqueSuffix}'
var appInsightsName = '${appName}-appinsights-${uniqueSuffix}'
var logAnalyticsWorkspaceName = '${appName}-law-${uniqueSuffix}'
var appServicePlanName = '${appName}-plan-${uniqueSuffix}'

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Storage Account for Functions runtime
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
  }
}

// Cosmos DB Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
  }
}

// Cosmos DB Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: 'UkApprenticeships'
  properties: {
    resource: {
      id: 'UkApprenticeships'
    }
  }
}

// Cosmos DB Container
resource cosmosContainer_Vacancies 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'Vacancies'
  properties: {
    resource: {
      id: 'Vacancies'
      partitionKey: {
        paths: [
          '/partitionKey'
        ]
        kind: 'Hash'
      }
      defaultTtl: -1
    }
  }
}

resource cosmosContainer_Users 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = {
  parent: cosmosDatabase
  name: 'Users'
  properties: {
    resource: {
      id: 'Users'
      partitionKey: {
        paths: [
          '/partitionKey'
        ]
        kind: 'Hash'
      }
      defaultTtl: -1
    }
  }
}

// App Service Plan (Functions Consumption)
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: false
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
        {
          name: 'ApprenticeshipApi__BaseUrl'
          value: 'https://api.apprenticeships.education.gov.uk/'
        }
        {
          name: 'ApprenticeshipApi__SubscriptionKey'
          value: apprenticeshipApiKey
        }
        {
          name: 'ApprenticeshipApi__ApiVersion'
          value: '2'
        }
        {
          name: 'ApprenticeshipApi__PostedInLastNumberOfDays'
          value: '1'
        }
        {
          name: 'ApprenticeshipApi__PageSize'
          value: '50'
        }
        {
          name: 'CosmosDb__ConnectionString'
          value: 'AccountEndpoint=${cosmosAccount.properties.documentEndpoint};AccountKey=${cosmosAccount.listKeys().primaryMasterKey};'
        }
        {
          name: 'CosmosDb__DatabaseName'
          value: 'UkApprenticeships'
        }
        {
          name: 'CosmosDb__ContainerName'
          value: 'Vacancies'
        }
        {
          name: 'ProcessVacanciesFunctionInterval'
          value: '0 0 8 * * *'
        }
        {
          name: 'VacancySyncFunctionInterval'
          value: '0 0 7 * * *'
        }
        {
          name: 'WhatsApp__ApiVersion'
          value: 'v22.0'
        }
        {
          name: 'WhatsApp__PhoneNumberId'
          value: whatsAppPhoneNumberId
        }
        {
          name: 'WhatsApp__AccessToken'
          value: whatsAppAccessToken
        }
        {
          name: 'WhatsApp__TemplateName'
          value: whatsAppTemplateName
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
    }
    httpsOnly: true
  }
  dependsOn: [
    cosmosContainer_Users, cosmosContainer_Vacancies
  ]
}

output functionAppName string = functionApp.name
output functionAppId string = functionApp.id
output cosmosAccountEndpoint string = cosmosAccount.properties.documentEndpoint
output storageAccountName string = storageAccount.name
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
