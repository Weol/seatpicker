param location string = resourceGroup().location
param postfix string = uniqueString(resourceGroup().name)

module app 'app.bicep' = {
  name: 'app'
  params: {
    location: location
    postfix: postfix
  }
}

module keyvaultModule 'keyvault.bicep' = {
  name: 'keyvault'
  params: {
    location: location
    postfix: postfix
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
  }
}

var keyvaultReferenceFormat = '@Microsoft.KeyVault(VaultName=${keyvaultModule.outputs.keyvaultUri};SecretName={0})'

resource appsettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    ASPNETCORE_ENVIRONMENT: 'Production'

    App_AuthCertificateProvider__Base64Certificate: format(keyvaultReferenceFormat, 'AuthenticationCertificate')

    App_Discord__ClientId: format(keyvaultReferenceFormat, 'DiscordClientId')
    App_Discord__ClientSecret: format(keyvaultReferenceFormat, 'DiscordClientSecret')
    App_Discord__RedirectUri: 'https://${appService.properties.defaultHostName}/redirect-login'

    App_Wolverine__ServiceBusFQDN: serviceBusEndpoint
  }
}

output appServiceName string = appService.name
