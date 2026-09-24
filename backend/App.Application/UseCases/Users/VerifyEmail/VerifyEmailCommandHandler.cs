using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.VerifyEmail;

public class VerifyEmailCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<VerifyEmailCommand>
{
    public async Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenGenerator.Hash(request.Token);
        var user = await userRepository.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken);

        if (user is null || user.EmailVerificationTokenExpiry is null
            || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
        {
            return Result.Failure(Error.Validation(
                "EmailVerification.InvalidToken",
                "Verification link is invalid or expired."));
        }

        user.VerifyEmail();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
