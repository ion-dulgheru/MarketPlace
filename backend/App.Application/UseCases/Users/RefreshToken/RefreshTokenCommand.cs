using App.Application.Abstractions.Messaging;
using App.Contracts.Responses.Users;

namespace App.Application.UseCases.Users.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokensResponse>;