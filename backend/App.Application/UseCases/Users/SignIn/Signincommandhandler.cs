using App.Application.Abstractions;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.SignIn;

public class SignInCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator)
    : ICommandHandler<SignInCommand, string>
{
    public async Task<Result<string>> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<string>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Invalid email or password."));
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return Result.Success(token);
    }
}