using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.ResetPassword;

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IRefreshTokenGenerator tokenGenerator,
    ISecurityAuditLogger auditLogger,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var user = await userRepository.GetByPasswordResetTokenHashAsync(tokenHash, cancellationToken);

        if (user is null || user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
            await auditLogger.LogAsync(
                SecurityEventType.PasswordResetCompleted,
                success: false,
                details: "Invalid or expired reset token.",
                ct: cancellationToken);

            return Result.Failure(Error.Validation(
                "PasswordReset.InvalidToken",
                "Reset token is invalid or expired."));
        }

        user.ResetPassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

        // Whoever holds a refresh token issued before the reset must not stay signed in.
        foreach (var session in await userSessionRepository.GetActiveByUserIdAsync(user.Id, cancellationToken))
        {
            session.Redeem();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            SecurityEventType.PasswordResetCompleted,
            success: true,
            userId: user.Guid,
            email: user.Email,
            ct: cancellationToken);

        return Result.Success();
    }
}