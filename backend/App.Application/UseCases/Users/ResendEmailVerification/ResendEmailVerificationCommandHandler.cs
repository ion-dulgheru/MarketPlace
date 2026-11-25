using App.Application.Abstractions.Email;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.ResendEmailVerification;

/// <summary>Always succeeds, so the answer does not reveal whether the address has an account.</summary>
public class ResendEmailVerificationCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator tokenGenerator,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork) : ICommandHandler<ResendEmailVerificationCommand>
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(24);

    /// <summary>Per account: the rate limit is per IP, so it does not stop flooding one mailbox from many IPs.</summary>
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(1);

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || user.EmailVerification || SentRecently(user.EmailVerificationTokenExpiry, now))
        {
            return Result.Success();
        }

        var token = tokenGenerator.GenerateToken();
        user.SetEmailVerificationToken(tokenGenerator.Hash(token), now.Add(TokenLifetime));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(user.Email, token, cancellationToken);

        return Result.Success();
    }

    private static bool SentRecently(DateTime? tokenExpiry, DateTime now) =>
        tokenExpiry is not null && tokenExpiry.Value - TokenLifetime + Cooldown > now;
}
