param location string
param postfix string

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
    administratorLogin: 'admin' 
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
