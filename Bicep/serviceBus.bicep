param location string
param postfix string

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

output serviceBusConnectionString string = busSharedAccessKeyAuthRule.listKeys().primaryConnectionString
