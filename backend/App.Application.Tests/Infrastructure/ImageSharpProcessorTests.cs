using System.Text;
using App.Infrastructure.Images;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace App.Application.Tests.Infrastructure;

public class ImageSharpProcessorTests
{
    private readonly ImageSharpProcessor _processor;

    public ImageSharpProcessorTests()
    {
        var loggerMock = new Mock<ILogger<ImageSharpProcessor>>();
        _processor = new ImageSharpProcessor(loggerMock.Object);
    }

    [Fact]
    public async Task SanitizeAsync_WhenJpegHasExifMetadata_ShouldStripExifAndReturnProcessedImage()
    {
        using var original = new Image<Rgba32>(10, 10);
        var profile = new ExifProfile();
        profile.SetValue(ExifTag.Software, "ImageSharpTest");
        original.Metadata.ExifProfile = profile;

        using var input = new MemoryStream();
        await original.SaveAsJpegAsync(input);
        input.Position = 0;

        var result = await _processor.SanitizeAsync(input);

        Assert.NotNull(result);
        Assert.Equal(".jpg", result.Extension);
        Assert.Equal("image/jpeg", result.ContentType);

        using var reloaded = await Image.LoadAsync(result.Content);
        Assert.Null(reloaded.Metadata.ExifProfile);
    }

    [Fact]
    public async Task SanitizeAsync_WhenPlainTextStream_ShouldReturnNull()
    {
        using var textStream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));

        var result = await _processor.SanitizeAsync(textStream);

        Assert.Null(result);
    }

    [Fact]
    public async Task SanitizeAsync_WhenDimensionsExceedLimit_ShouldReturnNull()
    {
        using var largeImage = new Image<Rgba32>(6001, 10);
        using var stream = new MemoryStream();
        await largeImage.SaveAsJpegAsync(stream);
        stream.Position = 0;

        var result = await _processor.SanitizeAsync(stream);

        Assert.Null(result);
    }

    [Fact]
    public async Task SanitizeAsync_WhenPngImage_ShouldPreservePngFormat()
    {
        using var pngImage = new Image<Rgba32>(10, 10);
        using var pngStream = new MemoryStream();
        await pngImage.SaveAsPngAsync(pngStream);
        pngStream.Position = 0;

        var result = await _processor.SanitizeAsync(pngStream);

        Assert.NotNull(result);
        Assert.Equal(".png", result.Extension);
        Assert.Equal("image/png", result.ContentType);
    }
}
