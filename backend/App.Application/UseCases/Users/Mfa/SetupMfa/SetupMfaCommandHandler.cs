using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Mfa;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Mfa.SetupMfa;

public class SetupMfaCommandHandler(
    IUserRepository userRepository,
    ITotpService totpService,
    ITotpSecretProtector secretProtector,
    MfaFailureRecorder failureRecorder,
    IUnitOfWork unitOfWork) : ICommandHandler<SetupMfaCommand, SetupMfaResponse>
{
    public async Task<Result<SetupMfaResponse>> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUuidAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<SetupMfaResponse>(MfaErrors.UserNotFound);
        }

        if (user.TwoFactorEnabled)
        {
            return Result.Failure<SetupMfaResponse>(MfaErrors.AlreadyEnabled);
        }

        var now = DateTime.UtcNow;

        if (user.IsLockedOut(now))
        {
            return Result.Failure<SetupMfaResponse>(MfaErrors.AccountLocked);
        }

        // A stolen access token must not be enough to enable MFA and lock the owner out.
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            await failureRecorder.RecordAsync(user, now, "setup", cancellationToken);
            return Result.Failure<SetupMfaResponse>(MfaErrors.InvalidPassword);
        }

        var secret = totpService.GenerateSecret();
        user.StartMfaSetup(secretProtector.Protect(secret));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SetupMfaResponse(totpService.BuildOtpAuthUri(user.Email, secret), secret));
    }
}
