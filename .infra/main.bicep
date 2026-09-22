targetScope = 'subscription'

@description('Deployment environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('Primary Azure region for all regional resources')
param location string = 'swedencentral'

@description('Administrator password for Azure PostgreSQL Flexible Server')
@secure()
param dbAdminPassword string

@description('Optional override for backend API container image')
param apiImage string = ''

@description('Optional override for frontend web container image')
param webImage string = ''

var baseName = 'marketplace'
var uniqueSuffix = substring(uniqueString(subscription().id, environmentName), 0, 6)
var resourceGroupName = '${baseName}-${environmentName}'

var identityName = '${baseName}-identity-${environmentName}'
var acrName = '${baseName}acr${uniqueSuffix}${environmentName}'
var keyVaultName = 'kv-${baseName}-${uniqueSuffix}'
var storageAccountName = '${baseName}st${uniqueSuffix}'
var postgresServerName = '${baseName}-psql-${uniqueSuffix}-${environmentName}'
var appServicePlanName = '${baseName}-plan-${environmentName}'
var webAppApiName = '${baseName}-api-${uniqueSuffix}-${environmentName}'
var webAppWebName = '${baseName}-web-${uniqueSuffix}-${environmentName}'

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

// 7. App Service Plan (Linux, container-capable)
module appServicePlan 'modules/app-service-plan.bicep' = {
  name: 'deploy-app-service-plan'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: appServicePlanName
    location: location
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 8. Backend API Web App
// Bootstrap placeholder — ACR has no image on first deploy. The CI pipeline
// swaps in the real image via `az webapp config container set` right after this runs.
var defaultImage = !empty(apiImage) ? apiImage : 'mcr.microsoft.com/k8se/quickstart:latest'

module webAppApi 'modules/backend-webapp.bicep' = {
  name: 'deploy-webapp-api'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: webAppApiName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    managedIdentityId: identity.outputs.id
    managedIdentityClientId: identity.outputs.clientId
    acrLoginServer: acr.outputs.loginServer
    acrUsername: acr.outputs.adminUsername
    acrPassword: acr.outputs.adminPassword
    image: defaultImage
    keyVaultUri: keyVault.outputs.vaultUri
    storageBlobEndpoint: storage.outputs.blobEndpoint
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// 9. Frontend Web App (SSR server)
var defaultWebImage = !empty(webImage) ? webImage : 'mcr.microsoft.com/k8se/quickstart:latest'

module webAppWeb 'modules/frontend-webapp.bicep' = {
  name: 'deploy-webapp-web'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: webAppWebName
    location: location
    appServicePlanId: appServicePlan.outputs.id
    acrLoginServer: acr.outputs.loginServer
    acrUsername: acr.outputs.adminUsername
    acrPassword: acr.outputs.adminPassword
    image: defaultWebImage
    tags: commonTags
  }
  dependsOn: [
    rg
  ]
}

// Outputs
output resourceGroupName string = resourceGroupName
output managedIdentityId string = identity.outputs.id
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
output webAppApiName string = webAppApi.outputs.name
output webAppApiHostname string = webAppApi.outputs.defaultHostname
output webAppWebName string = webAppWeb.outputs.name
output webAppWebHostname string = webAppWeb.outputs.defaultHostname
