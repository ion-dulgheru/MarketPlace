using App.Domain.Enums;

namespace App.Application.Abstractions.Interfaces;

/// <summary>
/// Records security events (logins, lockouts, MFA, denied admin access) for SR-13.
/// Implementations must never throw: a failed audit write must not break the request.
/// Never pass passwords, tokens or MFA codes in <paramref name="details"/>.
/// </summary>
public interface ISecurityAuditLogger
{
    Task LogAsync(
        SecurityEventType eventType,
        bool success,
        Guid? userId = null,
        string? email = null,
        string? details = null,
        CancellationToken ct = default);
}
