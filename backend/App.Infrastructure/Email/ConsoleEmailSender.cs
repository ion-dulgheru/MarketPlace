using App.Application.Abstractions.Email;

namespace App.Infrastructure.Email;

public class ConsoleEmailSender : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default)
    {
        var link = $"http://localhost:5173/reset-password?token={token}";
        Console.WriteLine($"[DEV EMAIL] Password reset for {toEmail}: {link}");
        return Task.CompletedTask;
    }
}