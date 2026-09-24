using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.VerifyEmail;

public record VerifyEmailCommand(string Token) : ICommand;
