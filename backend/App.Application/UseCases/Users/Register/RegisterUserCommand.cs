using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.Register;

public record RegisterUserCommand(string Email, string Password) : ICommand<Guid>;