param location string
param postfix string
param subnetId string
param dnsZoneId string

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

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2022-11-01' = {
  name: '${appService.name}-pe'
  location: location
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${appService.name}-pe'
        properties: {
          privateLinkServiceId: appService.id
          groupIds: [
            'sites'
          ]
        }
      }
    ]
  }

  resource dnsZoneGroup 'privateDnsZoneGroups@2022-11-01' = {
    name: 'default'
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'config1'
          properties: {
            privateDnsZoneId: dnsZoneId
          }
        }
      ]
    }
  } 
}

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

    App_MassTransit__ServiceBusFQDN: serviceBus.properties.serviceBusEndpoint 
  }
}

output appPrincipalId string = appService.identity.principalId
