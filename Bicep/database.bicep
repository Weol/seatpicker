param location string
param postfix string

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
      passwordAuth: 'Disabled'
      tenantId: tenant().tenantId
    }
    storage: {
      tier: 'P4'
      storageSizeGB: 32
      autoGrow: 'Enabled'
    }
    version: '15'
  }
}
