param name string
param location string
param appServicePlanId string
param managedIdentityId string
param managedIdentityClientId string
param acrLoginServer string
param acrUsername string
@secure()
param acrPassword string
param image string
param keyVaultUri string
param storageBlobEndpoint string
param tags object = {}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  kind: 'app,linux,container'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOCKER|${image}'
      alwaysOn: true
      healthCheckPath: '/health/ready'
      appSettings: [
        {
          name: 'WEBSITES_PORT'
          value: '8080'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://${acrLoginServer}'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: acrUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: acrPassword
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentityClientId
        }
        {
          name: 'KeyVault__Uri'
          value: keyVaultUri
        }
        {
          name: 'Storage__BlobEndpoint'
          value: storageBlobEndpoint
        }
      ]
    }
  }
  tags: tags
}

output id string = webApp.id
output name string = webApp.name
output defaultHostname string = webApp.properties.defaultHostName
