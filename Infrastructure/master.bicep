param location string = resourceGroup().location
param postfix string = uniqueString(resourceGroup().name)

@secure()
param databaseAdminPassword string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: 'storage${postfix}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {}
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value}'

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

resource api 'Microsoft.Web/sites@2018-02-01' = {
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

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: 'keyvault-${postfix}'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableSoftDelete: true
    enableRbacAuthorization: true
  }
}

resource dbServer 'Microsoft.Sql/servers@2022-02-01-preview' = {
  location: location
  name: 'dbserver-${postfix}'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: 'CloudSA0b670279'
    administratorLoginPassword: databaseAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: 'erik.nysto.rahka@itera.com'
      sid: 'b386dac1-d488-44c4-8dab-391517679fca'
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource database 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  location: location
  name: 'database-${postfix}'
  parent: dbServer
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin3_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}

/**
resource signalr 'Microsoft.SignalRService/signalR@2022-02-01' = {
  name: 'signalr-${postfix}' 
  location: location
  sku: {
    tier: 'Standard'
    name: 'Standard_S1'
    capacity: 1
  }
  kind: 'SignalR'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'false'
      }
      {
        flag: 'EnableMessagingLogs'
        value: 'false'
      }
      {
        flag: 'EnableLiveTrace'
        value: 'false'
      }
    ]
  }
}
*/

/*
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'servicebus-${postfix}'
  location: location
  sku: {
    name: 'Standard'
  }
}
*/

/*
resource frontend 'Microsoft.Web/staticSites@2022-03-01' = {
  name: 'frontend-${postfix}'
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    allowConfigFileUpdates: true
    stagingEnvironmentPolicy: 'Disabled'
  }

  resource appsettings 'config' = {
    name: 'appsettings'
    properties: {
      BACKEND_URL: 'https://${api.properties.defaultHostName}/api'
    }
  }
}
*/

resource appsettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'appsettings'
  parent: api
  properties: {
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    FUNCTIONS_EXTENSION_VERSION: '~4'
  }
}
