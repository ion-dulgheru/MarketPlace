using App.Application.Abstractions.Mfa;
using App.Domain.Entities;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Mfa;

/// <summary>
/// Checks a TOTP code against the user's secret, rejecting codes that were already used.
/// Does not change the user; on success returns the time step the caller must record.
/// </summary>
public class MfaCodeVerifier(ITotpService totpService, ITotpSecretProtector secretProtector)
{
    public Result<long> Verify(User user, string code)
    {
        if (user.TotpSecret is null)
        {
            return Result.Failure<long>(MfaErrors.SetupNotStarted);
        }

        var secret = secretProtector.Unprotect(user.TotpSecret);

        if (secret is null)
        {
            return Result.Failure<long>(MfaErrors.SecretUnavailable);
        }

        if (!totpService.Verify(secret, code, out var step) || user.IsTotpStepUsed(step))
        {
            return Result.Failure<long>(MfaErrors.InvalidCode);
        }

        return Result.Success(step);
    }
}
