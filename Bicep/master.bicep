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

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: 'plan-${postfix}'
  location: location
  sku: {
    name: 'D1'
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

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'servicebus-${postfix}'
  location: location
  sku: {
    name: 'Standard'
  }
}

resource busSharedAccessKeyAuthRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2017-04-01' = {
  parent: serviceBus
  name: 'SharedAccessKey'
  properties: {
    rights: [
      'Send'
      'Listen'
      'Manage'
    ]
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

var keyvaultReferenceFormat = '@Microsoft.KeyVault(VaultName=${keyvaultModule.outputs.keyvaultName};SecretName={0})'

resource appsettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    ASPNETCORE_ENVIRONMENT: 'Production'

    App_AuthCertificateProvider__Base64Certificate: format(keyvaultReferenceFormat, 'AuthenticationCertificate')

    App_Discord__ClientId: format(keyvaultReferenceFormat, 'DiscordClientId')
    App_Discord__ClientSecret: format(keyvaultReferenceFormat, 'DiscordClientSecret')
    App_Discord__RedirectUri: 'https://${appService.properties.defaultHostName}/redirect-login'

    App_MassTransit__ServiceBusConnectionString: busSharedAccessKeyAuthRule.listKeys().primaryConnectionString

    App_SeatRepository__StorageConnectionString: storageAccountConnectionString
  }
}

output appServiceName string = appService.name
