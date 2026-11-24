using App.Application.Abstractions;
using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.SignIn;

public class SignInCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    ISecurityAuditLogger auditLogger,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SignInCommand, SignInResponse>
{
    private static readonly TimeSpan MfaTokenLifetime = TimeSpan.FromMinutes(5);

    public async Task<Result<SignInResponse>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
        {
            await auditLogger.LogAsync(
                SecurityEventType.LoginFailed,
                success: false,
                email: request.Email,
                details: "User does not exist.",
                ct: cancellationToken);

            return Result.Failure<SignInResponse>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        if (user.IsLockedOut(now))
        {
            await auditLogger.LogAsync(
                SecurityEventType.AccountLocked,
                success: false,
                userId: user.Guid,
                email: user.Email,
                details: "Account is locked out.",
                ct: cancellationToken);

            return Result.Failure<SignInResponse>(Error.Forbidden(
                "Auth.AccountLocked",
                "This account is temporarily locked due to too many failed login attempts. Try again later."));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var lockedNow = user.IsLockedOut(now);
            if (lockedNow)
            {
                await auditLogger.LogAsync(
                    SecurityEventType.AccountLocked,
                    success: false,
                    userId: user.Guid,
                    email: user.Email,
                    details: "Account locked after reaching max failed login attempts.",
                    ct: cancellationToken);
            }
            else
            {
                await auditLogger.LogAsync(
                    SecurityEventType.LoginFailed,
                    success: false,
                    userId: user.Guid,
                    email: user.Email,
                    details: "Invalid password.",
                    ct: cancellationToken);
            }

            return Result.Failure<SignInResponse>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        if (!user.EmailVerification)
        {
            await auditLogger.LogAsync(
                SecurityEventType.LoginFailed,
                success: false,
                userId: user.Guid,
                email: user.Email,
                details: "Email not verified.",
                ct: cancellationToken);

            return Result.Failure<SignInResponse>(Error.Forbidden(
                "Auth.EmailNotVerified",
                "Please verify your email address before signing in."));
        }

        if (user.TwoFactorEnabled)
        {
            // The failed-attempt counter is deliberately not reset here: otherwise someone who knows the
            // password could reset it before each batch of code guesses and never reach the lockout.
            var mfaToken = refreshTokenGenerator.GenerateToken();
            user.SetMfaToken(refreshTokenGenerator.Hash(mfaToken), now.Add(MfaTokenLifetime));
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new SignInResponse(null, null, MfaRequired: true, MfaToken: mfaToken));
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

        await auditLogger.LogAsync(
            SecurityEventType.LoginSucceeded,
            success: true,
            userId: user.Guid,
            email: user.Email,
            ct: cancellationToken);

        return Result.Success(new SignInResponse(accessToken, refreshToken));
    }
}