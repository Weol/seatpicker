param location string
param postfix string

param readers array

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'kv-${postfix}'
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

var readerRole = '4633458b-17de-408a-b874-0445c86b69e6'
resource roleAssignmnets 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for reader in readers: {
  scope: keyVault
  name: guid(keyVault.id, reader, readerRole)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', readerRole)
    principalId: reader
    principalType: 'ServicePrincipal'
  }
}]

output keyvaultUri string = keyVault.properties.vaultUri
output keyvaultName string = keyVault.name
