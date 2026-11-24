using App.Application.Abstractions.Interfaces;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Application.UseCases.Users.SignIn;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Mfa.VerifyMfaLogin;

public class VerifyMfaLoginCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    MfaCodeVerifier codeVerifier,
    MfaFailureRecorder failureRecorder,
    ISecurityAuditLogger auditLogger,
    IUnitOfWork unitOfWork) : ICommandHandler<VerifyMfaLoginCommand, SignInResponse>
{
    private const int RefreshTokenExpiryDays = 30;

    public async Task<Result<SignInResponse>> Handle(VerifyMfaLoginCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await userRepository.GetByMfaTokenHashAsync(
            refreshTokenGenerator.Hash(request.MfaToken), cancellationToken);

        if (user is null || !user.TwoFactorEnabled || !user.IsMfaTokenValid(now))
        {
            return Result.Failure<SignInResponse>(MfaErrors.InvalidLoginToken);
        }

        if (user.IsLockedOut(now))
        {
            return Result.Failure<SignInResponse>(MfaErrors.AccountLocked);
        }

        var verification = codeVerifier.Verify(user, request.Code);

        if (verification.IsFailure)
        {
            if (verification.Error == MfaErrors.InvalidCode)
            {
                await failureRecorder.RecordAsync(user, now, "login", cancellationToken);
            }

            return Result.Failure<SignInResponse>(verification.Error);
        }

        user.RegisterTotpStep(verification.Value);
        user.ConsumeMfaToken();
        user.RegisterSuccessfulLogin();

        var (accessToken, jwtId) = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = refreshTokenGenerator.GenerateToken();

        await userSessionRepository.AddAsync(
            UserSession.Create(
                user.Id,
                refreshTokenGenerator.Hash(refreshToken),
                jwtId,
                now.AddDays(RefreshTokenExpiryDays)),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            SecurityEventType.LoginSucceeded, true, user.Guid, user.Email, "MFA", cancellationToken);

        return Result.Success(new SignInResponse(accessToken, refreshToken));
    }
}
