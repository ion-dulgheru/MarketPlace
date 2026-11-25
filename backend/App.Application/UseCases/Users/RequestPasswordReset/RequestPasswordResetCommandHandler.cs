using App.Application.Abstractions.Email;
using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.RequestPasswordReset;

public class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    ISecurityAuditLogger auditLogger,
    IUnitOfWork unitOfWork) : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is not null)
        {
            var token = tokenGenerator.GenerateToken();
            user.SetPasswordResetToken(tokenGenerator.Hash(token), DateTime.UtcNow.AddMinutes(30));
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await emailSender.SendPasswordResetEmailAsync(user.Email, token, cancellationToken);

            await auditLogger.LogAsync(
                SecurityEventType.PasswordResetRequested,
                success: true,
                userId: user.Guid,
                email: user.Email,
                ct: cancellationToken);
        }
        else
        {
            await auditLogger.LogAsync(
                SecurityEventType.PasswordResetRequested,
                success: false,
                email: request.Email,
                details: "Password reset requested for non-existent email.",
                ct: cancellationToken);
        }

        return Result.Success();
    }
}