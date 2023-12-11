param location string = resourceGroup().location
param postfix string = uniqueString(resourceGroup().name)

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

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'plan-${postfix}'
  location: location
  sku: {
    name: 'S1'
  }
}

resource appService 'Microsoft.Web/sites@2018-02-01' existing = {
  name: 'webapp-${postfix}'
}

resource stagingSlot 'Microsoft.Web/sites/slots@2018-02-01' = {
  name: 'staging'
  parent: appService
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    clientAffinityEnabled: false
    siteConfig: {
      alwaysOn: true
      webSocketsEnabled: true
      cors: {
        allowedOrigins: [
          'https://norwayeast.livediagnostics.monitor.azure.com'
        ]
      }
      virtualApplications: [
        {
          virtualPath: '/'
          physicalPath: 'site\\wwwroot'
        }
        {
          virtualPath: '/api'
          physicalPath: 'site\\wwwroot\\api'
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
      stagingSlot.identity.principalId
      appService.identity.principalId
    ]
  }
}

module databaseModule 'database.bicep' = {
  name: 'database'
  params: {
    location: location
    postfix: postfix
    keyvaultName: keyvaultModule.outputs.keyvaultName
  }
}

resource appsettings 'Microsoft.Web/sites/slots/config@2021-03-01' = {
  name: 'appsettings'
  parent: stagingSlot
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    ApplicationInsightsAgent_EXTENSION_VERSION: '~2'
    ASPNETCORE_ENVIRONMENT: 'Production'

    App_Database__Port: '5432'
    App_Database__Host: databaseModule.outputs.host
    App_Database__Name: databaseModule.outputs.dbName
    App_Keyvault__Uri: keyvaultModule.outputs.keyvaultUri
  }
}

output appServiceName string = appService.name
output stagingSlotName string = stagingSlot.name
output appInsConnectionString string = appInsights.properties.ConnectionString
