targetScope = 'subscription'

@description('Deployment environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('Primary Azure region for all regional resources')
param location string = 'swedencentral'

@description('Administrator password for Azure PostgreSQL Flexible Server')
@secure()
param dbAdminPassword string

@description('Container image tag to deploy')
param imageTag string = 'latest'

@description('Optional override for backend API container image')
param apiImage string = ''

@description('Optional override for frontend web container image')
param webImage string = ''

var baseName = 'marketplace'
var uniqueSuffix = substring(uniqueString(subscription().id, environmentName), 0, 6)
var resourceGroupName = '${baseName}-${environmentName}'

var identityName = '${baseName}-identity-${environmentName}'
var acrName = '${baseName}acr${uniqueSuffix}${environmentName}'
var keyVaultName = 'kv-mkt-${uniqueSuffix}-${environmentName}'
var storageAccountName = '${baseName}st${uniqueSuffix}'
var postgresServerName = '${baseName}-psql-${uniqueSuffix}-${environmentName}'
var containerAppEnvName = '${baseName}-cae-${environmentName}'
var logAnalyticsName = '${baseName}-log-${environmentName}'
var containerAppName = '${baseName}-api-${environmentName}'
var containerAppWebName = '${baseName}-web-${environmentName}'

var commonTags = {
  Project: 'MarketPlace'
  Environment: environmentName
  ManagedBy: 'Bicep'
}

// 1. Resource Group
module rg 'modules/resource-group.bicep' = {
  name: 'deploy-resource-group'
  params: {
    name: resourceGroupName
    location: location
    tags: commonTags
  }
}

// 2. User-Assigned Managed Identity
module identity 'modules/managed-identity.bicep' = {
  name: 'deploy-managed-identity'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: identityName
    location: location
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 3. Azure Container Registry
module acr 'modules/acr.bicep' = {
  name: 'deploy-acr'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: acrName
    location: location
    principalId: identity.outputs.principalId
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 4. Azure Key Vault (RBAC)
module keyVault 'modules/key-vault.bicep' = {
  name: 'deploy-key-vault'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: keyVaultName
    location: location
    principalId: identity.outputs.principalId
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 5. Azure Storage Account
module storage 'modules/storage.bicep' = {
  name: 'deploy-storage'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: storageAccountName
    location: location
    principalId: identity.outputs.principalId
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 6. Azure Database for PostgreSQL Flexible Server
module postgres 'modules/postgres.bicep' = {
  name: 'deploy-postgres'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: postgresServerName
    location: location
    administratorLoginPassword: dbAdminPassword
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 7. Container Apps Environment & Log Analytics
module containerAppEnv 'modules/container-app-env.bicep' = {
  name: 'deploy-container-app-env'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: containerAppEnvName
    logAnalyticsName: logAnalyticsName
    location: location
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 8. Azure Container App (Backend API)
var defaultImage = !empty(apiImage) ? apiImage : '${acr.outputs.loginServer}/marketplace-api:${imageTag}'

module containerApp 'modules/container-app.bicep' = {
  name: 'deploy-container-app'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: containerAppName
    location: location
    managedEnvironmentId: containerAppEnv.outputs.id
    managedIdentityId: identity.outputs.id
    managedIdentityClientId: identity.outputs.clientId
    acrLoginServer: acr.outputs.loginServer
    image: defaultImage
    keyVaultUri: keyVault.outputs.vaultUri
    storageBlobEndpoint: storage.outputs.blobEndpoint
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}
// 9. Azure Container App (Frontend SSR server)
var defaultWebImage = !empty(webImage) ? webImage : '${acr.outputs.loginServer}/marketplace-web:${imageTag}'

module containerAppWeb 'modules/frontend-container-app.bicep' = {
  name: 'deploy-container-app-web'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: containerAppWebName
    location: location
    managedEnvironmentId: containerAppEnv.outputs.id
    managedIdentityId: identity.outputs.id
    acrLoginServer: acr.outputs.loginServer
    image: defaultWebImage
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// Outputs
output resourceGroupName string = resourceGroupName
output managedIdentityClientId string = identity.outputs.clientId
output managedIdentityPrincipalId string = identity.outputs.principalId
output acrLoginServer string = acr.outputs.loginServer
output acrName string = acr.outputs.name
output keyVaultName string = keyVault.outputs.name
output keyVaultUri string = keyVault.outputs.vaultUri
output storageAccountName string = storage.outputs.name
output storageBlobEndpoint string = storage.outputs.blobEndpoint
output postgresServerFqdn string = postgres.outputs.fqdn
output postgresServerName string = postgres.outputs.serverName
output postgresDatabaseName string = postgres.outputs.databaseName
output containerAppName string = containerApp.outputs.name
output containerAppFqdn string = containerApp.outputs.fqdn
output containerAppWebName string = containerAppWeb.outputs.name
output containerAppWebFqdn string = containerAppWeb.outputs.fqdn
