using System.Security.Cryptography;
using System.Text;
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
    IUnitOfWork unitOfWork)
    : ICommandHandler<SignInCommand, SignInResponse>
{
    public async Task<Result<SignInResponse>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<SignInResponse>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        var (accessToken, jwtId) = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        var refreshTokenHash = HashToken(refreshToken);

        var session = new UserSession(
            user.Id,
            refreshTokenHash,
            jwtId,
            DateTime.UtcNow.AddDays(30));

        await userSessionRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SignInResponse(accessToken, refreshToken));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}