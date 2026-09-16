using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Users;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.SignIn;

public class SignInCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUserSessionRepository userSessionRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SignInCommand, AuthTokensResponse>
{
    private static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password.");

    private const int RefreshTokenExpiryDays = 30;

    public async Task<Result<AuthTokensResponse>> Handle(SignInCommand request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is not null && user.IsLockedOut(now))
        {
            return Result.Failure<AuthTokensResponse>(InvalidCredentials);
        }

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            if (user is not null)
            {
                user.RegisterFailedLogin(now);
                await unitOfWork.SaveChangesAsync(ct);
            }

            return Result.Failure<AuthTokensResponse>(InvalidCredentials);
        }

        user.RegisterSuccessfulLogin();

        var (accessToken, jwtId) = jwtTokenGenerator.GenerateToken(user);

        var refreshToken = refreshTokenGenerator.GenerateToken();
        var refreshTokenHash = refreshTokenGenerator.Hash(refreshToken);

        var session = UserSession.Create(
            user.Id,
            refreshTokenHash,
            jwtId,
            now.AddDays(RefreshTokenExpiryDays));

        await userSessionRepository.AddAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AuthTokensResponse(accessToken, refreshToken));
    }
}