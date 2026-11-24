using App.Application.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace App.Infrastructure.Images;

/// <summary>
/// Re-encodes uploaded images, so only pixels are kept: EXIF, GPS and any other embedded data are dropped.
/// </summary>
public sealed class SkiaImageProcessor(ILogger<SkiaImageProcessor> logger) : IImageProcessor
{
    private const int MaxDimension = 6000;
    private const int Quality = 85;

    public async Task<ProcessedImage?> SanitizeAsync(Stream input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.CanSeek)
        {
            input.Position = 0;
        }

        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, ct);
        using var data = SKData.CreateCopy(buffer.ToArray());
        using var codec = SKCodec.Create(data);

        if (codec is null)
        {
            return null;
        }

        var format = codec.EncodedFormat;
        (string Extension, string ContentType)? output = format switch
        {
            SKEncodedImageFormat.Jpeg => (".jpg", "image/jpeg"),
            SKEncodedImageFormat.Png => (".png", "image/png"),
            SKEncodedImageFormat.Webp => (".webp", "image/webp"),
            _ => null
        };

        if (output is null)
        {
            logger.LogWarning("Rejected image format {Format}.", format);
            return null;
        }

        // Only the header has been read so far: check the size before allocating memory for the pixels.
        if (codec.Info.Width > MaxDimension || codec.Info.Height > MaxDimension)
        {
            logger.LogWarning(
                "Image dimensions {Width}x{Height} exceed the maximum of {Max}x{Max2}.",
                codec.Info.Width, codec.Info.Height, MaxDimension, MaxDimension);
            return null;
        }

        using var bitmap = Decode(codec);

        if (bitmap is null)
        {
            logger.LogWarning("Failed to decode {Format} image.", format);
            return null;
        }

        using var oriented = ApplyOrientation(bitmap, codec.EncodedOrigin);
        using var image = SKImage.FromBitmap(oriented ?? bitmap);
        using var encoded = image.Encode(format, Quality);

        if (encoded is null)
        {
            logger.LogWarning("Failed to encode {Format} image.", format);
            return null;
        }

        return new ProcessedImage(
            new MemoryStream(encoded.ToArray()), output.Value.Extension, output.Value.ContentType);
    }

    private static SKBitmap? Decode(SKCodec codec)
    {
        // The color profile is dropped with the rest of the metadata, so convert the pixels to sRGB.
        using var srgb = SKColorSpace.CreateSrgb();
        var alphaType = codec.Info.AlphaType == SKAlphaType.Opaque ? SKAlphaType.Opaque : SKAlphaType.Premul;
        var bitmap = new SKBitmap();

        if (bitmap.TryAllocPixels(new SKImageInfo(
                codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, alphaType, srgb)) &&
            codec.GetPixels(bitmap.Info, bitmap.GetPixels()) == SKCodecResult.Success)
        {
            return bitmap;
        }

        bitmap.Dispose();
        return null;
    }

    /// <summary>Bakes the EXIF orientation into the pixels. Returns null if there is nothing to change.</summary>
    private static SKBitmap? ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        int w = source.Width, h = source.Height;
        SKMatrix? matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
            _ => null
        };

        if (matrix is null)
        {
            return null;
        }

        var swapsSides = matrix.Value.ScaleX == 0;
        var result = new SKBitmap(source.Info.WithSize(swapsSides ? h : w, swapsSides ? w : h));
        result.Erase(SKColors.Transparent);
        using var canvas = new SKCanvas(result);
        canvas.SetMatrix(matrix.Value);
        using var image = SKImage.FromBitmap(source);
        canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        return result;
    }
}
