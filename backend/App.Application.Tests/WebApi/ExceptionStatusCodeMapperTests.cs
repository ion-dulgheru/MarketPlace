using App.WebApi.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace App.Application.Tests.WebApi;

public class ExceptionStatusCodeMapperTests
{
    [Fact]
    public void Map_WhenUnauthorizedAccessException_ShouldReturn401()
    {
        var statusCode = ExceptionStatusCodeMapper.Map(new UnauthorizedAccessException());

        Assert.Equal(StatusCodes.Status401Unauthorized, statusCode);
    }

    [Fact]
    public void Map_WhenOperationCanceledException_ShouldReturn499()
    {
        var statusCode = ExceptionStatusCodeMapper.Map(new OperationCanceledException());

        Assert.Equal(ExceptionStatusCodeMapper.ClientClosedRequest, statusCode);
    }

    [Fact]
    public void Map_WhenTaskCanceledException_ShouldReturn499()
    {
        var statusCode = ExceptionStatusCodeMapper.Map(new TaskCanceledException());

        Assert.Equal(ExceptionStatusCodeMapper.ClientClosedRequest, statusCode);
    }

    [Fact]
    public void Map_WhenAnyOtherException_ShouldReturn500()
    {
        var statusCode = ExceptionStatusCodeMapper.Map(new InvalidOperationException());

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
    }
}
