using App.Domain.Common;
using App.Domain.Enums;

namespace App.Domain.Entities;

public class User : PublicEntity
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailVerification { get; private set; }
    public string? EmailVerificationTokenHash { get; private set; }
    public DateTime? EmailVerificationTokenExpiry { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string? PasswordResetTokenHash { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }
    public string? TotpSecret { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public long? LastTotpStep { get; private set; }
    public string? MfaTokenHash { get; private set; }
    public DateTime? MfaTokenExpiry { get; private set; }
    public UserRole Role { get; private set; } = UserRole.User;
    private User() { }

    public static User Create(string email, string passwordHash)
    {
        return new User
        {
            Email = email,
            PasswordHash = passwordHash
        };
    }

    public bool IsLockedOut(DateTime now) => LockedUntil is not null && LockedUntil > now;

    public void RegisterFailedLogin(DateTime now)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockedUntil = now.Add(LockoutDuration);
        }
    }

    public void RegisterSuccessfulLogin()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }
    public void SetPasswordResetToken(string tokenHash, DateTime expiresAt)
{
    PasswordResetTokenHash = tokenHash;
    PasswordResetTokenExpiry = expiresAt;
}

public void ResetPassword(string newPasswordHash)
{
    PasswordHash = newPasswordHash;
    PasswordResetTokenHash = null;
    PasswordResetTokenExpiry = null;
    // A login started with the old password must not be finishable after the password changes.
    ConsumeMfaToken();
}

public void SetEmailVerificationToken(string tokenHash, DateTime expiresAt)
{
    EmailVerificationTokenHash = tokenHash;
    EmailVerificationTokenExpiry = expiresAt;
}

public void VerifyEmail()
{
    EmailVerification = true;
    EmailVerificationTokenHash = null;
    EmailVerificationTokenExpiry = null;
}

    /// <summary>Stores a new (encrypted) TOTP secret. MFA stays off until <see cref="EnableMfa"/>.</summary>
    public void StartMfaSetup(string protectedSecret)
    {
        if (TwoFactorEnabled)
        {
            throw new InvalidOperationException("MFA is already enabled.");
        }

        TotpSecret = protectedSecret;
        LastTotpStep = null;
    }

    public void EnableMfa(long totpStep)
    {
        if (TotpSecret is null)
        {
            throw new InvalidOperationException("MFA setup has not been started.");
        }

        TwoFactorEnabled = true;
        LastTotpStep = totpStep;
    }

    public void DisableMfa()
    {
        TwoFactorEnabled = false;
        TotpSecret = null;
        LastTotpStep = null;
        ConsumeMfaToken();
    }

    /// <summary>Anti-replay: a code is accepted only if its time step is newer than the last accepted one.</summary>
    public bool IsTotpStepUsed(long totpStep) => LastTotpStep is not null && totpStep <= LastTotpStep;

    public void RegisterTotpStep(long totpStep) => LastTotpStep = totpStep;

    public void SetMfaToken(string tokenHash, DateTime expiresAt)
    {
        MfaTokenHash = tokenHash;
        MfaTokenExpiry = expiresAt;
    }

    public bool IsMfaTokenValid(DateTime now) => MfaTokenHash is not null && MfaTokenExpiry > now;

    public void ConsumeMfaToken()
    {
        MfaTokenHash = null;
        MfaTokenExpiry = null;
    }
}