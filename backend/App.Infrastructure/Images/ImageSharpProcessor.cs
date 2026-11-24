using App.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace App.Infrastructure.Images;

public sealed class ImageSharpProcessor(ILogger<ImageSharpProcessor> logger) : IImageProcessor
{
    private const int MaxDimension = 6000;

    public async Task<ProcessedImage?> SanitizeAsync(Stream input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        Stream seekableStream = input;
        MemoryStream? bufferedStream = null;

        try
        {
            if (!input.CanSeek)
            {
                bufferedStream = new MemoryStream();
                await input.CopyToAsync(bufferedStream, ct);
                bufferedStream.Position = 0;
                seekableStream = bufferedStream;
            }
            else
            {
                seekableStream.Position = 0;
            }

            IImageFormat? format;
            try
            {
                format = await Image.DetectFormatAsync(seekableStream, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to detect image format from stream.");
                return null;
            }

            if (format is null)
            {
                return null;
            }

            var mimeType = format.DefaultMimeType?.ToLowerInvariant();
            var isAllowedFormat = format == JpegFormat.Instance
                                  || format == PngFormat.Instance
                                  || format == WebpFormat.Instance
                                  || mimeType is "image/jpeg" or "image/png" or "image/webp";

            if (!isAllowedFormat)
            {
                logger.LogWarning("Rejected image format '{FormatName}' with MIME '{MimeType}'.", format.Name, mimeType);
                return null;
            }

            seekableStream.Position = 0;

            Image image;
            try
            {
                image = await Image.LoadAsync(seekableStream, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load image even though format was detected as {FormatName}.", format.Name);
                return null;
            }

            using (image)
            {
                if (image.Width > MaxDimension || image.Height > MaxDimension)
                {
                    logger.LogWarning(
                        "Image dimensions {Width}x{Height} exceed maximum allowed {MaxWidth}x{MaxHeight} (potential decompression bomb).",
                        image.Width, image.Height, MaxDimension, MaxDimension);
                    return null;
                }

                image.Mutate(x => x.AutoOrient());

                image.Metadata.ExifProfile = null;
                image.Metadata.IptcProfile = null;
                image.Metadata.XmpProfile = null;

                var outputStream = new MemoryStream();
                string extension;
                string contentType;

                if (format == PngFormat.Instance || mimeType == "image/png")
                {
                    await image.SaveAsPngAsync(outputStream, new PngEncoder(), ct);
                    extension = ".png";
                    contentType = "image/png";
                }
                else if (format == WebpFormat.Instance || mimeType == "image/webp")
                {
                    await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 85 }, ct);
                    extension = ".webp";
                    contentType = "image/webp";
                }
                else
                {
                    await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 }, ct);
                    extension = ".jpg";
                    contentType = "image/jpeg";
                }

                outputStream.Position = 0;
                return new ProcessedImage(outputStream, extension, contentType);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while sanitizing image.");
            return null;
        }
        finally
        {
            bufferedStream?.Dispose();
        }
    }
}
