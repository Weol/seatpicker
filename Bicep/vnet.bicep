param location string
param postfix string

var serviceEndpoints = [
  'Microsoft.Sql'
  'Microsoft.Keyvault'
]

resource nsg 'Microsoft.Network/networkSecurityGroups@2022-07-01' = {
  name: 'seatpicker-nsg-${postfix}'
  location: location
  properties: {
    securityRules: [
      {
        name: 'DenyInternetOutBound'
        properties: {
          access: 'Deny'
          description: 'Deny internet outbound traffic'
          destinationAddressPrefix: 'Internet'
          destinationPortRange: '*'
          direction: 'Outbound'
          priority: 100 
          protocol: 'Any'
          sourceAddressPrefix: '*'
          sourcePortRange: '*'
        }
      }
    ]
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2021-05-01' = {
  name: 'vnet-${postfix}'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    subnets: [
      {
        name: 'seatpicker' 
        properties: {
          addressPrefix: '10.0.1.0/24'
          serviceEndpoints: [for serviceEndpoint in serviceEndpoints: {
              locations: [ location ]
              service: serviceEndpoint
          }]
          networkSecurityGroup: {
            id: nsg.id
          }
        }
      }
    ]
  }
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2021-05-01' = {
  name: 'seatpicker' 
  parent: vnet
  properties: {
    addressPrefix: '10.0.1.0/24'
    serviceEndpoints: [for serviceEndpoint in serviceEndpoints: {
        locations: [ location ]
        service: serviceEndpoint
    }]
    networkSecurityGroup: {
      id: nsg.id
    }
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink.azurewebsites.net'
  location: 'global'
  
  resource link 'virtualNetworkLinks@2020-06-01' = {
    name: 'link'
    location: 'global'
    properties: {
      registrationEnabled: false
      virtualNetwork: {
        id: vnet.id
      }
    }
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2020-06-01' = {
  name: 'kre-${env}-debtcollection-apigateway-webapp-pe'
  location: location
  properties: {
    subnet: {
      id: privateEndpointSubnet.id
    }
    privateLinkServiceConnections: [
      {
        name: 'kre-${env}-debtcollection-apigateway-webapp-pe'
        properties: {
          privateLinkServiceId: webApi.id
          groupIds: [
            'sites'
          ]
        }
      }
    ]
  }
}

resource privateEndpointDnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2021-05-01' = {
  name: 'dnsgroup'
  parent: privateEndpoint
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'config1'
        properties: {
          privateDnsZoneId: privateDnsZone.id
        }
      }
    ]
  }
} 

output subnetId string = subnet.id
