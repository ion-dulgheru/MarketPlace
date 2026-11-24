namespace App.WebApi.Infrastructure;

/// <summary>
/// Single responsibility: wire the global exception handling into the application,
/// so Program.cs only needs one line for services and one for the pipeline.
/// </summary>
public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
