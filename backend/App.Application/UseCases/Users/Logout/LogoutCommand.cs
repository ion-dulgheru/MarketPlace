using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.Logout;

public record LogoutCommand(string RefreshToken) : ICommand;
