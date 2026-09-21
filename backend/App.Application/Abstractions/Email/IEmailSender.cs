namespace App.Application.Abstractions.Email;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default);
}