using App.Domain.Common;

namespace App.Domain.Entities;

public class User : PublicEntity
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailVerification { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public string? PasswordResetTokenHash { get; private set; }
public DateTime? PasswordResetTokenExpiry { get; private set; }

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
}
}