using App.Application.Abstractions.Email;
using Microsoft.Extensions.Configuration;
using Resend;

namespace App.Infrastructure.Email;

public class ResendEmailSender(IResend resend, IConfiguration configuration) : IEmailSender
{
    public async Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var baseUrl = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var fromAddress = configuration["Email:FromAddress"] ?? "onboarding@resend.dev";
        var link = $"{baseUrl}/reset-password?token={token}";

        var message = new EmailMessage
        {
            From = fromAddress,
            Subject = "Reset your password",
            HtmlBody = $"<p>Click the link to reset your password: <a href=\"{link}\">{link}</a></p>"
        };
        message.To.Add(toEmail);

        await resend.EmailSendAsync(message, ct);
    }
    public async Task SendAdvertReportedEmailAsync(
    Guid advertUuid, string advertTitle, string reason, CancellationToken ct = default)
    {
    var operatorAddress = configuration["Email:OperatorAddress"] ?? "onboarding@resend.dev";
    var fromAddress = configuration["Email:FromAddress"] ?? "onboarding@resend.dev";

    var message = new EmailMessage
    {
        From = fromAddress,
        Subject = $"Advert reported: {advertTitle}",
        HtmlBody = $"<p>Advert <strong>{advertTitle}</strong> ({advertUuid}) was reported for: <strong>{reason}</strong>.</p>"
    };
    message.To.Add(operatorAddress);

    await resend.EmailSendAsync(message, ct);
    }
}
