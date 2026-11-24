using System.Text.Json;
using App.WebApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace App.Application.Tests.WebApi;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_WhenUnauthorized_ShouldReturn401ProblemDetails()
    {
        var context = CreateHttpContext();

        var handled = await _handler.TryHandleAsync(context, new UnauthorizedAccessException(), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(401, ReadBody(context).GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task TryHandleAsync_WhenUnexpectedException_ShouldReturn500WithTraceId()
    {
        var context = CreateHttpContext();
        context.TraceIdentifier = "trace-abc";

        await _handler.TryHandleAsync(context, new InvalidOperationException(), CancellationToken.None);

        var body = ReadBody(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("trace-abc", body.GetProperty("traceId").GetString());
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldNotLeakExceptionMessageOrStackTrace()
    {
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Connection string password=SECRET");

        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        var rawBody = ReadRawBody(context);
        Assert.DoesNotContain("SECRET", rawBody);
        Assert.DoesNotContain(nameof(InvalidOperationException), rawBody);
    }

    [Fact]
    public async Task TryHandleAsync_WhenRequestCanceled_ShouldReturn499WithoutBody()
    {
        var context = CreateHttpContext();

        var handled = await _handler.TryHandleAsync(context, new OperationCanceledException(), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(ExceptionStatusCodeMapper.ClientClosedRequest, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string ReadRawBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return new StreamReader(context.Response.Body).ReadToEnd();
    }

    private static JsonElement ReadBody(HttpContext context) =>
        JsonDocument.Parse(ReadRawBody(context)).RootElement;
}
