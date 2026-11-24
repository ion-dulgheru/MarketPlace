using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.Messaging;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Mfa.EnableMfa;

public class EnableMfaCommandHandler(
    IUserRepository userRepository,
    MfaCodeVerifier codeVerifier,
    ISecurityAuditLogger auditLogger,
    IUnitOfWork unitOfWork) : ICommandHandler<EnableMfaCommand>
{
    public async Task<Result> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUuidAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(MfaErrors.UserNotFound);
        }

        if (user.TwoFactorEnabled)
        {
            return Result.Failure(MfaErrors.AlreadyEnabled);
        }

        var verification = codeVerifier.Verify(user, request.Code);

        if (verification.IsFailure)
        {
            return Result.Failure(verification.Error);
        }

        user.EnableMfa(verification.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            SecurityEventType.MfaEnabled, true, user.Guid, user.Email, ct: cancellationToken);

        return Result.Success();
    }
}
