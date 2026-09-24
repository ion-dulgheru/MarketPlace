using App.Application.Abstractions;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.SignIn;

public class SignInCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SignInCommand, SignInResponse>
{
    public async Task<Result<SignInResponse>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<SignInResponse>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        if (user.IsLockedOut(now))
        {
            return Result.Failure<SignInResponse>(Error.Forbidden(
                "Auth.AccountLocked",
                "This account is temporarily locked due to too many failed login attempts. Try again later."));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<SignInResponse>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        if (!user.EmailVerification)
        {
            return Result.Failure<SignInResponse>(Error.Forbidden(
                "Auth.EmailNotVerified",
                "Please verify your email address before signing in."));
        }

        user.RegisterSuccessfulLogin();

        var (accessToken, jwtId) = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = refreshTokenGenerator.GenerateToken();
        var refreshTokenHash = refreshTokenGenerator.Hash(refreshToken);

        var session = new UserSession(
            user.Id,
            refreshTokenHash,
            jwtId,
            DateTime.UtcNow.AddDays(30));

        await userSessionRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SignInResponse(accessToken, refreshToken));
    }
}