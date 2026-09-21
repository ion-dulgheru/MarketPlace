using App.Application.Abstractions.Email;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.RequestPasswordReset;

public class RequestPasswordResetCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is not null)
        {
            var token = tokenGenerator.GenerateToken();
           user.SetPasswordResetToken(tokenGenerator.Hash(token), DateTime.UtcNow.AddMinutes(30));
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await emailSender.SendPasswordResetEmailAsync(user.Email, token, cancellationToken);
        }

        return Result.Success();
    }
}