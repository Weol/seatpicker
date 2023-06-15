param location string
param postfix string

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'servicebus-${postfix}'
  location: location
  sku: {
    name: 'Standard'
  }
}

output fullyQualifiedDomainName string = serviceBus.properties.serviceBusEndpoint
