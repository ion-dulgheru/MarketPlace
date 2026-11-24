using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.Mfa.DisableMfa;

public record DisableMfaCommand(Guid UserId, string Code) : ICommand;
