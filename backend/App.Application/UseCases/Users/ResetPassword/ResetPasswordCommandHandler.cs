using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.ResetPassword;

public class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var user = await userRepository.GetByPasswordResetTokenHashAsync(tokenHash, cancellationToken);

        if (user is null || user.PasswordResetTokenExpiry is null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
        {
return Result.Failure(Error.Validation(                "PasswordReset.InvalidToken",
                "Reset token is invalid or expired."));
        }

        user.ResetPassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}