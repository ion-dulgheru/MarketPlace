namespace App.Application.Abstractions.Mfa;

/// <summary>
/// Second-factor strategy based on time-based one-time passwords (RFC 6238).
/// </summary>
public interface ITotpService
{
    /// <summary>Generates a new Base32 secret.</summary>
    string GenerateSecret();

    /// <summary>Builds the otpauth:// URI encoded in the QR code scanned by authenticator apps.</summary>
    string BuildOtpAuthUri(string email, string secret);

    /// <param name="step">Time step the code belongs to; used by the caller to reject replays.</param>
    bool Verify(string secret, string code, out long step);
}
