namespace App.Domain.Enums;

/// <summary>
/// Security-relevant events recorded by <c>ISecurityAuditLogger</c> (SR-13).
/// Add new values at the end only: they are persisted by numeric value.
/// </summary>
public enum SecurityEventType
{
    LoginSucceeded,
    LoginFailed,
    AccountLocked,
    RefreshFailed,
    PasswordResetRequested,
    PasswordResetCompleted,
    MfaEnabled,
    MfaDisabled,
    MfaFailed,
    AdminAccessDenied
}
