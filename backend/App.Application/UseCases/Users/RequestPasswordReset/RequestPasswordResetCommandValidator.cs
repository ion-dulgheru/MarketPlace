using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.RequestPasswordReset;

public record RequestPasswordResetCommand(string Email) : ICommand;