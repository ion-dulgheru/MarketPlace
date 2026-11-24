using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.Mfa.EnableMfa;

public record EnableMfaCommand(Guid UserId, string Code) : ICommand;
