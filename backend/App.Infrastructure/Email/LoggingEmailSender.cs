using App.Application.Abstractions.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Email;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger, IConfiguration configuration) : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var baseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var link = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        logger.LogInformation("[DEV EMAIL] Password reset email for {Email}: {Link}", toEmail, link);
        return Task.CompletedTask;
    }

    public Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var baseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var link = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(token)}";
        logger.LogInformation("[DEV EMAIL] Email verification link for {Email}: {Link}", toEmail, link);
        return Task.CompletedTask;
    }

    public Task SendAdvertReportedEmailAsync(
        Guid advertUuid, string advertTitle, string reason, CancellationToken ct = default)
    {
        logger.LogInformation("[DEV EMAIL] Advert reported: {Title} ({Uuid}) for {Reason}", advertTitle, advertUuid, reason);
        return Task.CompletedTask;
    }
}
