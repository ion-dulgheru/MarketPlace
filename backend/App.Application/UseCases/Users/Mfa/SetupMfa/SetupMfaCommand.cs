using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.Mfa.SetupMfa;

public record SetupMfaCommand(Guid UserId, string CurrentPassword) : ICommand<SetupMfaResponse>;

public record SetupMfaResponse(string OtpAuthUri, string Secret);
