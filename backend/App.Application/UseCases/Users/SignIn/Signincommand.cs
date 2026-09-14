using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.SignIn;

public record SignInCommand(string Email, string Password) : ICommand<string>;