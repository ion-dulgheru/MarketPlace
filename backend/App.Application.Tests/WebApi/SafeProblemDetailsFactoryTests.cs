using App.WebApi.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace App.Application.Tests.WebApi;

public class SafeProblemDetailsFactoryTests
{
    [Fact]
    public void Create_ShouldSetStatusAndTraceId()
    {
        var problem = SafeProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, "trace-123");

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("trace-123", problem.Extensions["traceId"]);
    }

    [Fact]
    public void Create_ShouldNotExposeDetailOrInstance()
    {
        var problem = SafeProblemDetailsFactory.Create(StatusCodes.Status500InternalServerError, "trace-123");

        Assert.Null(problem.Detail);
        Assert.Null(problem.Instance);
    }

    [Theory]
    [InlineData(StatusCodes.Status401Unauthorized, "Authentication is required.")]
    [InlineData(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")]
    [InlineData(StatusCodes.Status400BadRequest, "Request could not be processed.")]
    public void Create_ShouldUseGenericTitle(int statusCode, string expectedTitle)
    {
        var problem = SafeProblemDetailsFactory.Create(statusCode, "trace-123");

        Assert.Equal(expectedTitle, problem.Title);
    }
}
