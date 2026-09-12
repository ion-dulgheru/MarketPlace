using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Register;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailExists = await userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure<Guid>(Error.Conflict(
                "User.EmailAlreadyExists",
                "An account with this email already exists."));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(request.Email, passwordHash);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Guid);
    }
}