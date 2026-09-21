using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : ICommand;