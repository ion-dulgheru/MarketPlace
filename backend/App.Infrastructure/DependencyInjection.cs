using App.Application.Abstractions;
using App.Application.Abstractions.Interfaces;
using App.Infrastructure.Auth;
using App.Infrastructure.Services;
using App.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Mfa;
using App.Application.Abstractions.Email;
using App.Infrastructure.Email;
using App.Infrastructure.Images;
using App.Application.Abstractions.Captcha;
using App.Infrastructure.Captcha;
using Azure.Communication.Email;
namespace App.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
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
        services.AddOpenKeyDataProtection(configuration);
        services.AddSingleton<ITotpService, TotpService>();
        services.AddSingleton<ITotpSecretProtector, TotpSecretProtector>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();
        services.AddScoped<ISecurityAuditLogger, DbSecurityAuditLogger>();
        services.AddHttpClient<GoogleRecaptchaVerifier>();
        services.AddTransient<ICaptchaVerifier, GoogleRecaptchaVerifier>();
        var emailConnString = configuration["Azure:CommunicationServices:ConnectionString"];
        if (string.IsNullOrWhiteSpace(emailConnString) && environment.IsDevelopment())
        {
            // Development only: it logs password reset and verification links.
            services.AddScoped<IEmailSender, LoggingEmailSender>();
        }
        else
        {
            // Fail fast: the send happens inside a user request and a failure is only logged.
            var emailOptions = new EmailClientOptions();
            emailOptions.Retry.MaxRetries = 1;
            emailOptions.Retry.NetworkTimeout = TimeSpan.FromSeconds(5);
            services.AddSingleton(_ => new EmailClient(emailConnString, emailOptions));
            services.AddScoped<AzureEmailSender>();
            services.AddScoped<IEmailSender>(sp => new ResilientEmailSender(
                sp.GetRequiredService<AzureEmailSender>(),
                sp.GetRequiredService<ILogger<ResilientEmailSender>>()));
        }
        return services;
    }
}