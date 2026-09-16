using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Users;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserSessionRepository userSessionRepository,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefreshTokenCommand, AuthTokensResponse>
{
    private static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "Invalid or expired refresh token.");

    private const int RefreshTokenExpiryDays = 30;

    public async Task<Result<AuthTokensResponse>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var incomingHash = refreshTokenGenerator.Hash(command.RefreshToken);

        var session = await userSessionRepository.GetByRefreshTokenHashAsync(incomingHash, ct);

        if (session is null || !session.IsValid(now))
        {
            return Result.Failure<AuthTokensResponse>(InvalidRefreshToken);
        }

        var user = await userRepository.GetByIdAsync(session.UserId, ct);

        if (user is null)
        {
            return Result.Failure<AuthTokensResponse>(InvalidRefreshToken);
        }

        session.Redeem();

        var (accessToken, jwtId) = jwtTokenGenerator.GenerateToken(user);

        var newRefreshToken = refreshTokenGenerator.GenerateToken();
        var newRefreshTokenHash = refreshTokenGenerator.Hash(newRefreshToken);

        var newSession = Domain.Entities.UserSession.Create(
            user.Id,
            newRefreshTokenHash,
            jwtId,
            now.AddDays(RefreshTokenExpiryDays));

        await userSessionRepository.AddAsync(newSession, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AuthTokensResponse(accessToken, newRefreshToken));
    }
}