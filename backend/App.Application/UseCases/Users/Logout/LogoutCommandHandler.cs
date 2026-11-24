using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Logout;

public class LogoutCommandHandler(
    IUserSessionRepository userSessionRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var session = await userSessionRepository.GetByRefreshTokenHashAsync(
            refreshTokenGenerator.Hash(request.RefreshToken), cancellationToken);

        // Logout is idempotent: an unknown or already used token is not an error.
        if (session is null || session.Redeemed)
        {
            return Result.Success();
        }

        session.Redeem();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
