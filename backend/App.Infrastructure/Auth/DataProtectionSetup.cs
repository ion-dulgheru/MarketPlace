using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure.Auth;

internal static class DataProtectionSetup
{
    // Private container, created by .infra/modules/storage.bicep. Never reuse the images container:
    // that one is publicly readable.
    private const string KeysContainer = "dataprotection-keys";
    private const string KeysBlob = "keys.xml";

    /// <summary>
    /// Keys must outlive the container instance, otherwise every restart would make the stored TOTP
    /// secrets unreadable. In Azure they are kept in a private blob; locally, in the user profile.
    /// </summary>
    public static IServiceCollection AddOpenKeyDataProtection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddDataProtection().SetApplicationName("OpenKey");
        var connectionString = configuration["Storage:ConnectionString"] ?? configuration["Storage--ConnectionString"];
        var blobEndpoint = configuration["Storage:BlobEndpoint"] ?? configuration["Storage--BlobEndpoint"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            builder.PersistKeysToAzureBlobStorage(connectionString, KeysContainer, KeysBlob);
        }
        else if (!string.IsNullOrWhiteSpace(blobEndpoint))
        {
            var clientId = configuration["AZURE_CLIENT_ID"] ?? configuration["Azure:ClientId"];
            var credential = !string.IsNullOrWhiteSpace(clientId)
                ? new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId })
                : new DefaultAzureCredential();

            var blobClient = new BlobServiceClient(new Uri(blobEndpoint), credential)
                .GetBlobContainerClient(KeysContainer)
                .GetBlobClient(KeysBlob);
            builder.PersistKeysToAzureBlobStorage(blobClient);
        }

        return services;
    }
}
