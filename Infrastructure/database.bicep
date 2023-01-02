
param location string = resourceGroup().location
param postfix string = uniqueString(resourceGroup().name)

@secure()
param databaseAdminPassword string

resource dbServer 'Microsoft.Sql/servers@2022-02-01-preview' = {
  location: location
  name: 'dbserver-${postfix}'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    administratorLogin: 'CloudSA0b670279'
    administratorLoginPassword: databaseAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: 'erik.nysto.rahka@itera.com'
      sid: 'b386dac1-d488-44c4-8dab-391517679fca'
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
    restrictOutboundNetworkAccess: 'Disabled'
  }
}

resource database 'Microsoft.Sql/servers/databases@2022-02-01-preview' = {
  location: location
  name: 'database-${postfix}'
  parent: dbServer
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    collation: 'SQL_Latin3_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}
