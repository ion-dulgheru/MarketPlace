using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using App.Infrastructure.Images;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharp;

namespace App.Application.Tests.Infrastructure;

public class SkiaImageProcessorTests
{
    private const string Secret = "GPS-SECRET";

    private readonly Mock<ILogger<SkiaImageProcessor>> _logger = new();
    private readonly SkiaImageProcessor _processor;

    public SkiaImageProcessorTests() => _processor = new SkiaImageProcessor(_logger.Object);

    [Fact]
    public async Task SanitizeAsync_WhenJpegHasExifMetadata_ShouldStripExif()
    {
        var input = WithExif(Encode(10, 10, SKEncodedImageFormat.Jpeg), orientation: 1);
        Assert.Contains(Secret, Encoding.Latin1.GetString(input));

        var result = await Sanitize(input);

        Assert.NotNull(result);
        Assert.Equal(".jpg", result.Extension);
        Assert.Equal("image/jpeg", result.ContentType);
        var output = Encoding.Latin1.GetString(Bytes(result.Content));
        Assert.DoesNotContain(Secret, output);
        Assert.DoesNotContain("Exif", output);
    }

    // The source is 40x20 with a red top-left quadrant, so each orientation moves it to a different place.
    [Theory]
    [InlineData(1, 40, 20, 10, 5)]
    [InlineData(2, 40, 20, 30, 5)]
    [InlineData(3, 40, 20, 30, 15)]
    [InlineData(4, 40, 20, 10, 15)]
    [InlineData(5, 20, 40, 5, 10)]
    [InlineData(6, 20, 40, 15, 10)]
    [InlineData(7, 20, 40, 15, 30)]
    [InlineData(8, 20, 40, 5, 30)]
    public async Task SanitizeAsync_ShouldApplyExifOrientation(int orientation, int width, int height, int redX, int redY)
    {
        var result = await Sanitize(WithExif(Encode(40, 20, SKEncodedImageFormat.Jpeg), orientation));

        using var bitmap = SKBitmap.Decode(Bytes(result!.Content));
        Assert.Equal((width, height), (bitmap.Width, bitmap.Height));

        foreach (var (x, y) in new[] { (redX, redY), (width - redX, redY), (redX, height - redY), (width - redX, height - redY) })
        {
            var pixel = bitmap.GetPixel(x, y);
            var isRed = pixel.Red > 180 && pixel.Blue < 80;
            Assert.True(isRed == ((x, y) == (redX, redY)), $"unexpected color {pixel} at {x},{y}");
        }
    }

    [Theory]
    [InlineData(SKEncodedImageFormat.Png, ".png", "image/png")]
    [InlineData(SKEncodedImageFormat.Webp, ".webp", "image/webp")]
    [InlineData(SKEncodedImageFormat.Jpeg, ".jpg", "image/jpeg")]
    public async Task SanitizeAsync_ShouldPreserveFormat(SKEncodedImageFormat format, string extension, string contentType)
    {
        var result = await Sanitize(Encode(10, 10, format));

        Assert.NotNull(result);
        Assert.Equal(extension, result.Extension);
        Assert.Equal(contentType, result.ContentType);
        using var codec = SKCodec.Create(new MemoryStream(Bytes(result.Content)));
        Assert.Equal(format, codec.EncodedFormat);
    }

    [Fact]
    public async Task SanitizeAsync_ShouldKeepTransparency()
    {
        using var source = new SKBitmap(4, 4);
        source.Erase(SKColors.Transparent);
        using var data = SKImage.FromBitmap(source).Encode(SKEncodedImageFormat.Png, 100);

        var result = await Sanitize(data.ToArray());

        using var bitmap = SKBitmap.Decode(Bytes(result!.Content));
        Assert.Equal(0, bitmap.GetPixel(1, 1).Alpha);
    }

    [Fact]
    public async Task SanitizeAsync_WhenDimensionsExceedLimit_ShouldReturnNull()
    {
        Assert.Null(await Sanitize(Encode(6001, 10, SKEncodedImageFormat.Jpeg)));
    }

    [Fact]
    public async Task SanitizeAsync_WhenDecompressionBomb_ShouldRejectItFromTheHeader()
    {
        // A few hundred bytes declaring 30000x30000 pixels (3.6 GB decoded). The pixel data is missing,
        // so a processor that decodes first would report a decoding failure, not a size problem.
        var result = await Sanitize(PngHeader(30000, 30000));

        Assert.Null(result);
        _logger.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("exceed")),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task SanitizeAsync_WhenPlainText_ShouldReturnNull()
    {
        Assert.Null(await Sanitize("hello"u8.ToArray()));
    }

    [Fact]
    public async Task SanitizeAsync_WhenEmpty_ShouldReturnNull()
    {
        Assert.Null(await Sanitize([]));
    }

    [Fact]
    public async Task SanitizeAsync_WhenGif_ShouldReturnNull()
    {
        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        Assert.Null(await Sanitize(gif));
    }

    [Fact]
    public async Task SanitizeAsync_WhenTruncated_ShouldReturnNull()
    {
        var png = Encode(200, 200, SKEncodedImageFormat.Png);

        Assert.Null(await Sanitize(png[..(png.Length / 2)]));
    }

    private Task<App.Application.Abstractions.Interfaces.ProcessedImage?> Sanitize(byte[] content) =>
        _processor.SanitizeAsync(new MemoryStream(content));

    private static byte[] Bytes(Stream stream) => ((MemoryStream)stream).ToArray();

    private static byte[] Encode(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Blue);
        using (var canvas = new SKCanvas(bitmap))
        using (var paint = new SKPaint())
        {
            paint.Color = SKColors.Red;
            canvas.DrawRect(0, 0, width / 2f, height / 2f, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
        return data.ToArray();
    }

    /// <summary>Inserts an EXIF segment (orientation + a marker text) right after the JPEG start marker.</summary>
    private static byte[] WithExif(byte[] jpeg, int orientation)
    {
        var text = Encoding.ASCII.GetBytes(Secret + "\0");
        using var tiff = new MemoryStream();
        tiff.Write("MM\0*"u8);
        WriteUInt32(tiff, 8);
        WriteUInt16(tiff, 2);
        WriteUInt16(tiff, 0x0112); // Orientation, SHORT
        WriteUInt16(tiff, 3);
        WriteUInt32(tiff, 1);
        WriteUInt16(tiff, (ushort)orientation);
        WriteUInt16(tiff, 0);
        WriteUInt16(tiff, 0x0131); // Software, ASCII
        WriteUInt16(tiff, 2);
        WriteUInt32(tiff, (uint)text.Length);
        WriteUInt32(tiff, 8 + 2 + 2 * 12 + 4);
        WriteUInt32(tiff, 0);
        tiff.Write(text);

        using var output = new MemoryStream();
        output.Write(jpeg.AsSpan(0, 2));
        output.Write([0xFF, 0xE1]);
        WriteUInt16(output, (ushort)(2 + 6 + tiff.Length));
        output.Write("Exif\0\0"u8);
        output.Write(tiff.ToArray());
        output.Write(jpeg.AsSpan(2));
        return output.ToArray();
    }

    private static byte[] PngHeader(int width, int height)
    {
        using var output = new MemoryStream();
        output.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

        using var header = new MemoryStream();
        WriteUInt32(header, (uint)width);
        WriteUInt32(header, (uint)height);
        header.Write([8, 6, 0, 0, 0]);
        WriteChunk(output, "IHDR"u8, header.ToArray());

        using var pixels = new MemoryStream();
        using (var zlib = new ZLibStream(pixels, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(new byte[64]);
        }

        WriteChunk(output, "IDAT"u8, pixels.ToArray());
        WriteChunk(output, "IEND"u8, []);
        return output.ToArray();
    }

    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, byte[] data)
    {
        WriteUInt32(output, (uint)data.Length);
        byte[] body = [.. type, .. data];
        output.Write(body);
        WriteUInt32(output, Crc32(body));
    }

    private static uint Crc32(byte[] data)
    {
        var crc = 0xFFFFFFFFu;

        foreach (var b in data)
        {
            crc ^= b;

            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }

        return ~crc;
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(bytes, value);
        stream.Write(bytes);
    }

    private static void WriteUInt32(Stream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
        stream.Write(bytes);
    }
}
