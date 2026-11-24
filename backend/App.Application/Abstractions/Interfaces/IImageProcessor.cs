namespace App.Application.Abstractions.Interfaces;

public interface IImageProcessor
{
    // Intoarce null daca continutul nu e o imagine permisa (jpeg/png/webp).
    Task<ProcessedImage?> SanitizeAsync(Stream input, CancellationToken ct = default);
}

public sealed record ProcessedImage(Stream Content, string Extension, string ContentType);
