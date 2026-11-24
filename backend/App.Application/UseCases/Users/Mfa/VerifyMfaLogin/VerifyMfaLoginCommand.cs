using App.Application.Abstractions.Messaging;
using App.Application.UseCases.Users.SignIn;

namespace App.Application.UseCases.Users.Mfa.VerifyMfaLogin;

public record VerifyMfaLoginCommand(string MfaToken, string Code) : ICommand<SignInResponse>;
