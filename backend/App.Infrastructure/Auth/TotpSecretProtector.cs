using System.Security.Cryptography;
using App.Application.Abstractions.Mfa;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Auth;

public sealed class TotpSecretProtector(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<TotpSecretProtector> logger) : ITotpSecretProtector
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("OpenKey.Totp");

    public string Protect(string secret) => _protector.Protect(secret);

    public string? Unprotect(string protectedSecret)
    {
        try
        {
            return _protector.Unprotect(protectedSecret);
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "A TOTP secret could not be decrypted; check the Data Protection key storage.");
            return null;
        }
    }
}
