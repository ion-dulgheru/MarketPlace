using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.ChangePassword;

public class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByUuidAsync(request.UserId, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(Error.Validation(
                "User.InvalidCurrentPassword",
                "Current password is incorrect."));
        }

        user.ResetPassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}