using App.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Configuration;

namespace App.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadFolder;
    private readonly string _urlPrefix;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredFolder = configuration["Storage:UploadFolder"];
        _uploadFolder = !string.IsNullOrWhiteSpace(configuredFolder)
            ? configuredFolder
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "adverts");

        var configuredPrefix = configuration["Storage:UrlPrefix"];
        _urlPrefix = !string.IsNullOrWhiteSpace(configuredPrefix)
            ? configuredPrefix.TrimEnd('/')
            : "/uploads/adverts";
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        if (!Directory.Exists(_uploadFolder))
        {
            Directory.CreateDirectory(_uploadFolder);
        }

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var physicalPath = Path.Combine(_uploadFolder, uniqueFileName);

        await using var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, ct);

        return $"{_urlPrefix}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        var fileName = Path.GetFileName(fileUrl);
        var physicalPath = Path.Combine(_uploadFolder, fileName);

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }
}
