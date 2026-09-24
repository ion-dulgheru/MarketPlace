using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.Register;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? PhoneNumber,
    string CaptchaToken
    ) : ICommand;