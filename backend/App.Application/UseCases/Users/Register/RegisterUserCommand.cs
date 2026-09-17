using App.Application.Abstractions.Messaging;
using App.Application.UseCases.Users.SignIn;

namespace App.Application.UseCases.Users.Register;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? PhoneNumber) : ICommand<SignInResponse>;