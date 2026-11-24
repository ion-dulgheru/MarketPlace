namespace App.Application.Abstractions.Mfa;

/// <summary>Encrypts TOTP secrets before they are stored in the database.</summary>
public interface ITotpSecretProtector
{
    string Protect(string secret);

    /// <summary>Returns null if the value cannot be decrypted (e.g. the encryption keys were lost).</summary>
    string? Unprotect(string protectedSecret);
}
