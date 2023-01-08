param location string
param postfix string

param contributors array

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'servicebus-${postfix}'
  location: location
  sku: {
    name: 'Standard'
  }
}

var ownerRole = '090c5cfd-751d-490a-894a-3ce6f1109419'
resource roleAssignments 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = [for contributor in contributors: {
  scope: serviceBus 
  name: guid(serviceBus.id, contributor, ownerRole)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', ownerRole)
    principalId: contributor
    principalType: 'ServicePrincipal'
  }
}]

output serviceBusEndpoint string = serviceBus.properties.serviceBusEndpoint
