using App.Application.Abstractions.Captcha;
using App.Application.Abstractions.Email;
using App.Application.Abstractions.JWT;
using App.Application.Abstractions.Messaging;
using App.Domain.Entities;
using App.Domain.Repositories;
using App.Domain.Shared;

namespace App.Application.UseCases.Users.Register;

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenGenerator tokenGenerator,
    ICaptchaVerifier captchaVerifier,
    IEmailSender emailSender,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand>
{
    private const double MinimumCaptchaScore = 0.5;
    private const int EmailVerificationTokenExpiryHours = 24;

    public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CaptchaToken))
        {
            return Result.Failure(Error.Validation(
                "Captcha.TokenMissing", "Captcha token is required."));
        }

        var captcha = await captchaVerifier.VerifyAsync(request.CaptchaToken, cancellationToken);

        if (!captcha.Success)
        {
            return Result.Failure(Error.Validation(
                "Captcha.InvalidToken", "Captcha token is invalid or expired."));
        }

        if (captcha.Score is null || captcha.Score < MinimumCaptchaScore)
        {
            return Result.Failure(Error.Forbidden(
                "Captcha.ScoreTooLow", "Captcha verification failed."));
        }

        var emailExists = await userRepository.EmailExistsAsync(request.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure(Error.Conflict(
                "User.EmailAlreadyExists",
                "An account with this email already exists."));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(request.Email, passwordHash);

        var verificationToken = tokenGenerator.GenerateToken();
        user.SetEmailVerificationToken(
            tokenGenerator.Hash(verificationToken),
            DateTime.UtcNow.AddHours(EmailVerificationTokenExpiryHours));

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var userDetails = UserDetails.Create(
            user.Id,
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.PhoneNumber);

        await userRepository.AddDetailsAsync(userDetails, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(user.Email, verificationToken, cancellationToken);

        return Result.Success();
    }
}