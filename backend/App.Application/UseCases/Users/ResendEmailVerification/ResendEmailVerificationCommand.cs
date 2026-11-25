using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.ResendEmailVerification;

public record ResendEmailVerificationCommand(string Email) : ICommand;
