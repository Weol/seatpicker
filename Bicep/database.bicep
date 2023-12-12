param location string
param postfix string
param keyvaultName string
param adminUsername string = 'CoolGuy95'
@secure()
param adminPassword string

resource database 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  location: location
  name: 'database-${postfix}'
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Enabled'
      tenantId: tenant().tenantId
    }
    administratorLogin: adminUsername
    administratorLoginPassword: adminPassword
    storage: {
      tier: 'P4'
      storageSizeGB: 32
      autoGrow: 'Enabled'
    }
    version: '15'
  }
}

resource production 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  name: 'production'
  parent: database
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' existing = {
  name: keyvaultName
}

resource usernameSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: keyVault
  name: 'DatabaseAdminUsername'
  properties: {
    value: adminUsername
  }
}

output dbName string = production.name
output host string = database.properties.fullyQualifiedDomainName
