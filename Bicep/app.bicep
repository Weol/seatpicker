param location string
param postfix string
param serviceBusEndpoint string
param keyvaultEndpoint string

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


output appPrincipalId string = appService.identity.principalId
