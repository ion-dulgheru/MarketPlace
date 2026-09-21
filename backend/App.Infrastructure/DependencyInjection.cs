using App.Application.Abstractions;
using App.Application.Abstractions.Interfaces;
using App.Infrastructure.Auth;
using App.Infrastructure.Services;
using App.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Email;
using App.Infrastructure.Email;
using Resend;
namespace App.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddHttpClient<ResendClient>();
services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = configuration["Resend:ApiKey"]!;
});
services.AddTransient<IResend, ResendClient>();
services.AddScoped<IEmailSender, ResendEmailSender>();
        return services;
    }
}