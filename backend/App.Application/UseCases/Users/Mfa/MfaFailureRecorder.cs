using App.Application.Abstractions.Interfaces;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;

namespace App.Application.UseCases.Users.Mfa;

/// <summary>
/// Counts a wrong code towards the account lockout, so a 6-digit code cannot be guessed
/// by spreading attempts over many IPs (the rate limit is per IP only).
/// </summary>
public class MfaFailureRecorder(ISecurityAuditLogger auditLogger, IUnitOfWork unitOfWork)
{
    public async Task RecordAsync(User user, DateTime now, string operation, CancellationToken ct)
    {
        user.RegisterFailedLogin(now);
        var locked = user.IsLockedOut(now);

        if (locked)
        {
            user.ConsumeMfaToken();
        }

        await unitOfWork.SaveChangesAsync(ct);
        await auditLogger.LogAsync(
            SecurityEventType.MfaFailed, false, user.Guid, user.Email, operation, ct);

        if (locked)
        {
            await auditLogger.LogAsync(
                SecurityEventType.AccountLocked, false, user.Guid, user.Email, "Too many wrong MFA codes", ct);
        }
    }
}
