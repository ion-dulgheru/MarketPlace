using App.Application.Abstractions.Auth;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator) 
    : ICommandHandler<LoginCommand, string>
{
    public async Task<Result<string>> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
        {
            return Result.Failure<string>(Error.Unauthorized(
                "Auth.InvalidCredentials",
                "Email or password is incorrect"));
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return Result.Success(token);
    }
}