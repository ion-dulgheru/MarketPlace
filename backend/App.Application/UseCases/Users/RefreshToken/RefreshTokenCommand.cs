using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;