using App.Application.Abstractions.Interfaces;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var containerName = configuration["Storage:ContainerName"] ?? "app-files";
        var connectionString = configuration["Storage:ConnectionString"] ?? configuration["Storage--ConnectionString"];
        var blobEndpoint = configuration["Storage:BlobEndpoint"] ?? configuration["Storage--BlobEndpoint"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var serviceClient = new BlobServiceClient(connectionString);
            _containerClient = serviceClient.GetBlobContainerClient(containerName);
        }
        else if (!string.IsNullOrWhiteSpace(blobEndpoint))
        {
            var uri = new Uri(blobEndpoint);
            var clientId = configuration["AZURE_CLIENT_ID"] ?? configuration["Azure:ClientId"];
            var credential = !string.IsNullOrWhiteSpace(clientId)
                ? new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = clientId })
                : new DefaultAzureCredential();

            var serviceClient = new BlobServiceClient(uri, credential);
            _containerClient = serviceClient.GetBlobContainerClient(containerName);
        }
        else
        {
            throw new InvalidOperationException(
                "Neither 'Storage:ConnectionString' nor 'Storage:BlobEndpoint' is configured for AzureBlobStorageService.");
        }
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueBlobName = $"listings/{Guid.NewGuid()}{extension}";
        var blobClient = _containerClient.GetBlobClient(uniqueBlobName);

        var contentType = GetContentType(extension);
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await blobClient.UploadAsync(stream, uploadOptions, ct);
        _logger.LogInformation("Uploaded image to Azure Blob Storage: {BlobUri}", blobClient.Uri.AbsoluteUri);

        return blobClient.Uri.AbsoluteUri;
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl)) return;

        try
        {
            if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            {
                var path = uri.AbsolutePath.TrimStart('/');
                var prefix = $"{_containerClient.Name}/";
                var blobName = path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? path.Substring(prefix.Length)
                    : path;

                var blobClient = _containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
                _logger.LogInformation("Deleted blob from Azure Blob Storage: {BlobName}", blobName);
            }
            else
            {
                var fileName = Path.GetFileName(fileUrl);
                var localPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "listings", fileName);
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete file {FileUrl}", fileUrl);
        }
    }

    private static string GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".gif" => "image/gif",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream"
    };
}
