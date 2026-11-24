using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.Messaging;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Mfa.DisableMfa;

public class DisableMfaCommandHandler(
    IUserRepository userRepository,
    MfaCodeVerifier codeVerifier,
    MfaFailureRecorder failureRecorder,
    ISecurityAuditLogger auditLogger,
    IUnitOfWork unitOfWork) : ICommandHandler<DisableMfaCommand>
{
    public async Task<Result> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await userRepository.GetByUuidAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(MfaErrors.UserNotFound);
        }

        if (!user.TwoFactorEnabled)
        {
            return Result.Failure(MfaErrors.NotEnabled);
        }

        if (user.IsLockedOut(now))
        {
            return Result.Failure(MfaErrors.AccountLocked);
        }

        var verification = codeVerifier.Verify(user, request.Code);

        if (verification.IsFailure)
        {
            if (verification.Error == MfaErrors.InvalidCode)
            {
                await failureRecorder.RecordAsync(user, now, "disable", cancellationToken);
            }

            return Result.Failure(verification.Error);
        }

        user.DisableMfa();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            SecurityEventType.MfaDisabled, true, user.Guid, user.Email, ct: cancellationToken);

        return Result.Success();
    }
}
