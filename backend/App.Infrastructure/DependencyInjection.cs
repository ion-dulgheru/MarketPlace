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
using App.Application.Abstractions.Captcha;
using App.Infrastructure.Captcha;
using Azure.Communication.Email;
namespace App.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        var hasAzureStorage = !string.IsNullOrWhiteSpace(configuration["Storage:ConnectionString"] ?? configuration["Storage--ConnectionString"])
            || !string.IsNullOrWhiteSpace(configuration["Storage:BlobEndpoint"] ?? configuration["Storage--BlobEndpoint"]);

        if (hasAzureStorage)
        {
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddHttpClient<GoogleRecaptchaVerifier>();
services.AddTransient<ICaptchaVerifier, GoogleRecaptchaVerifier>();
        services.AddSingleton(_ => new EmailClient(configuration["Azure:CommunicationServices:ConnectionString"]));
        services.AddScoped<IEmailSender, AzureEmailSender>();
        return services;
    }
}