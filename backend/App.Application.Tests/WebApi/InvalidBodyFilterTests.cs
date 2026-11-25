using App.WebApi.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace App.Application.Tests.WebApi;

public class InvalidBodyFilterTests
{
    [Fact]
    public void OnActionExecuting_WhenBodyIsNull_ShouldReturn400()
    {
        var context = CreateContext(BindingSource.Body, new() { ["request"] = null });

        new InvalidBodyFilter().OnActionExecuting(context);

        Assert.IsType<BadRequestObjectResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuting_WhenBodyWasNotBound_ShouldReturn400()
    {
        var context = CreateContext(BindingSource.Body, new());

        new InvalidBodyFilter().OnActionExecuting(context);

        Assert.IsType<BadRequestObjectResult>(context.Result);
    }

    [Fact]
    public void OnActionExecuting_WhenBodyIsBound_ShouldContinue()
    {
        var context = CreateContext(BindingSource.Body, new() { ["request"] = new object() });

        new InvalidBodyFilter().OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnActionExecuting_WhenQueryParameterIsNull_ShouldContinue()
    {
        var context = CreateContext(BindingSource.Query, new() { ["request"] = null });

        new InvalidBodyFilter().OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    private static ActionExecutingContext CreateContext(BindingSource source, Dictionary<string, object?> arguments)
    {
        var descriptor = new ActionDescriptor
        {
            Parameters =
            [
                new ParameterDescriptor
                {
                    Name = "request",
                    ParameterType = typeof(object),
                    BindingInfo = new BindingInfo { BindingSource = source }
                }
            ]
        };

        return new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), descriptor), [], arguments, new object());
    }
}
