param location string
param postfix string
param keyvaultName string
param adminUsername string = 'CoolGuy95'

resource passwordGenerator 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'generate-password-${postfix}'
  location: location
  kind: 'AzurePowerShell'
  properties: {
    azPowerShellVersion: '3.0' 
    retentionInterval: 'P1D'
    scriptContent: loadTextContent('./DatabasePasswordGenerator.ps1')
  }
}

resource dbServer 'Microsoft.Sql/servers@2022-02-01-preview' = {
  location: location
  name: 'dbserver-${postfix}'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: adminUsername
    administratorLoginPassword: passwordGenerator.properties.outputs.password
  }
}

resource database 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  location: location
  name: 'database-${postfix}'
  parent: dbServer
  sku: {
    name: 'Basic'
    tier: 'Basic'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyvaultName
}

resource passwordSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyVault
  name: 'DatabaseAdminPassword'
  properties: {
    value: passwordGenerator.properties.outputs.password
  }
}

resource usernameSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyVault
  name: 'DatabaseAdminUsername'
  properties: {
    value: adminUsername
  }
}
