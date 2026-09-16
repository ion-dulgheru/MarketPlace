using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Users;

namespace App.Application.UseCases.Users.SignIn;

public record SignInCommand(string Email, string Password) : ICommand<AuthTokensResponse>;