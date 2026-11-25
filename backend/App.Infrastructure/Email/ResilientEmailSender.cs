using App.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Email;

/// <summary>
/// Decorator: an email provider failure is logged instead of failing the operation, which is already saved by then.
/// It also keeps "forgot password" answers identical for existing and unknown addresses.
/// The recipient, the token and the link are never logged.
/// </summary>
public sealed class ResilientEmailSender(IEmailSender inner, ILogger<ResilientEmailSender> logger) : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string token, CancellationToken ct = default) =>
        TrySendAsync("password reset", () => inner.SendPasswordResetEmailAsync(toEmail, token, ct), ct);

    public Task SendEmailVerificationAsync(string toEmail, string token, CancellationToken ct = default) =>
        TrySendAsync("email verification", () => inner.SendEmailVerificationAsync(toEmail, token, ct), ct);

    public Task SendAdvertReportedEmailAsync(
        Guid advertUuid, string advertTitle, string reason, CancellationToken ct = default) =>
        TrySendAsync("advert reported", () => inner.SendAdvertReportedEmailAsync(advertUuid, advertTitle, reason, ct), ct);

    private async Task TrySendAsync(string kind, Func<Task> send, CancellationToken ct)
    {
        try
        {
            await send();
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Failed to send the {EmailKind} email", kind);
        }
    }
}
