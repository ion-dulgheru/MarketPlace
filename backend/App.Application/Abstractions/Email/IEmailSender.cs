namespace App.Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default);
    Task SendAdvertReportedEmailAsync(Guid advertUuid, string advertTitle, string reason, CancellationToken ct = default);
    Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken ct = default);
}