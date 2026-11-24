using App.Application.Abstractions.Mfa;
using OtpNet;

namespace App.Infrastructure.Auth;

public sealed class TotpService : ITotpService
{
    private const string Issuer = "OpenKey";
    private const int SecretSizeBytes = 20;

    public string GenerateSecret() => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(SecretSizeBytes));

    public string BuildOtpAuthUri(string email, string secret) =>
        $"otpauth://totp/{Issuer}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Issuer}";

    public bool Verify(string secret, string code, out long step)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code, out step, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
