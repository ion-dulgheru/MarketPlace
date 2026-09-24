using App.Application.Abstractions.Email;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;

namespace App.Infrastructure.Email;

public class AzureEmailSender(EmailClient emailClient, IConfiguration configuration) : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var baseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var link = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";
        return SendAsync(toEmail, "Reset your password",
            $"<p>Click the link to reset your password: <a href=\"{link}\">{link}</a></p>", ct);
    }

    public Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var baseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var link = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(token)}";
        return SendAsync(toEmail, "Verify your email",
            $"<p>Click the link to verify your email address: <a href=\"{link}\">{link}</a></p>", ct);
    }

    public Task SendAdvertReportedEmailAsync(
        Guid advertUuid, string advertTitle, string reason, CancellationToken ct = default)
    {
        var operatorAddress = configuration["Email:OperatorAddress"]
            ?? configuration["Azure:CommunicationServices:SenderAddress"]!;

        return SendAsync(operatorAddress, $"Advert reported: {advertTitle}",
            $"<p>Advert <strong>{advertTitle}</strong> ({advertUuid}) was reported for: <strong>{reason}</strong>.</p>",
            ct);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        var senderAddress = configuration["Azure:CommunicationServices:SenderAddress"]!;

        var emailContent = new EmailContent(subject) { Html = htmlBody };
        var emailMessage = new EmailMessage(senderAddress, toEmail, emailContent);

        await emailClient.SendAsync(WaitUntil.Completed, emailMessage, ct);
    }
}