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

resource appService 'Microsoft.Web/sites@2018-02-01' = {
  name: 'saltenlan'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    httpsOnly: true
    serverFarmId: appServicePlan.id
    siteConfig: {
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

resource stagingSlot 'Microsoft.Web/sites/slots@2018-02-01' = {
  name: 'staging'
  parent: appService
  location: location
  properties: {
    httpsOnly: true
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
  }
}

var keyvaultReferenceFormat = '@Microsoft.KeyVault(VaultName=${keyvaultModule.outputs.keyvaultUri};SecretName={0})'

resource appsettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    ASPNETCORE_ENVIRONMENT: 'Production'

    App_Keyvault__Uri: format(keyvaultReferenceFormat, 'AuthenticationCertificate')
  }
}

output appServiceName string = appService.name
