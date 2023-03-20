param location string = resourceGroup().location
param postfix string = uniqueString(resourceGroup().name)

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: 'storage${postfix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {}
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: 'loganalytics-${postfix}'
  location: location
  properties: {
    retentionInDays: 720
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appins-${postfix}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource appService 'Microsoft.Web/sites@2018-02-01' = {
  name: 'api-${postfix}'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v6.0'
      appSettings: [
        // If these two appsettings are not here then deployment fails because it says its missing WEBSITE_CONTENTSHARE
        // Therefore we have to duplicate WEBSITE_CONTENTAZUREFILECONNECTIONSTRING here as well as further down
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: 'api-${postfix}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageAccountConnectionString
        }
      ]
    }
  }
}

module keyvaultModule 'keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    postfix: postfix
    readers: [
      appService.identity.principalId
    ]
  }
}

resource appsettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    App_Keyvault__Uri: keyvaultModule.outputs.keyvaultUri
  }
}

output appServiceName string = appService.name
