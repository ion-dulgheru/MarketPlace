using App.Infrastructure.Services;
using Xunit;

namespace App.Application.Tests.Infrastructure;

public class HtmlSanitizerServiceTests
{
    private readonly HtmlSanitizerService _service = new();

    [Fact]
    public void Sanitize_WhenSafeFormattingUsed_ShouldPreserveTags()
    {
        var html = "<p>This is <strong>bold</strong> and <em>italic</em> text.</p>";
        var result = _service.Sanitize(html);

        Assert.Contains("<strong>bold</strong>", result);
        Assert.Contains("<em>italic</em>", result);
        Assert.Contains("<p>", result);
    }

    [Fact]
    public void Sanitize_WhenListUsed_ShouldPreserveListTags()
    {
        var html = "<ul><li>First</li><li>Second</li></ul>";
        var result = _service.Sanitize(html);

        Assert.Contains("<ul>", result);
        Assert.Contains("<li>First</li>", result);
        Assert.Contains("<li>Second</li>", result);
        Assert.Contains("</ul>", result);
    }

    [Fact]
    public void Sanitize_WhenMaliciousScriptTagPresent_ShouldStripScript()
    {
        var html = "<p>Clean text</p><script>alert('xss');</script>";
        var result = _service.Sanitize(html);

        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("alert('xss');", result);
        Assert.Contains("Clean text", result);
    }

    [Fact]
    public void Sanitize_WhenEventHandlerPresent_ShouldStripHandler()
    {
        var html = "<p onclick=\"stealData()\">Click me</p><img src=\"x\" onerror=\"stealData()\">";
        var result = _service.Sanitize(html);

        Assert.DoesNotContain("onclick", result);
        Assert.DoesNotContain("onerror", result);
        Assert.DoesNotContain("stealData()", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Sanitize_WhenEmptyOrWhitespace_ShouldReturnEmpty(string? input)
    {
        var result = _service.Sanitize(input!);
        Assert.Equal(string.Empty, result);
    }
}
