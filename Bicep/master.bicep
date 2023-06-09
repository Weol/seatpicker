param location string = resourceGroup().location
param postfix string = uniqueString(resourceGroup().name)

module vnetModule 'vnet.bicep' = {
  name: 'vnet'
  params: {
    location: location
    postfix: postfix
  }
}

module app 'app.bicep' = {
  name: 'app'
  params: {
    location: location
    postfix: postfix
    subnetId: vnetModule.outputs.subnetId
  }
}

module keyvaultModule 'keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    postfix: postfix
    subnetId: vnetModule.outputs.subnetId
    readers: [
      app.outputs.appPrincipalId
    ]
  }
}

module databaseModule 'database.bicep' = {
  name: 'database'
  params: {
    location: location
    postfix: postfix
    keyvaultName: keyvaultModule.outputs.keyvaultName
    subnetId: vnetModule.outputs.subnetId
  }
}

output appServiceName string = appService.name
