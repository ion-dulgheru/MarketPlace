using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Application.UseCases.Users.SignIn;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Register;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, SignInResponse>
{
    private const int RefreshTokenExpiryDays = 30;

    public async Task<Result<SignInResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure<SignInResponse>(Error.Conflict(
                "User.EmailAlreadyExists",
                "An account with this email already exists."));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(request.Email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userDetails = UserDetails.Create(
            user.Id,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber);

        await userRepository.AddDetailsAsync(userDetails, cancellationToken);

        var (accessToken, jwtId) = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = refreshTokenGenerator.GenerateToken();
        var refreshTokenHash = refreshTokenGenerator.Hash(refreshToken);

        var session = UserSession.Create(
            user.Id,
            refreshTokenHash,
            jwtId,
            DateTime.UtcNow.AddDays(RefreshTokenExpiryDays));

        await userSessionRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SignInResponse(accessToken, refreshToken));
    }
}