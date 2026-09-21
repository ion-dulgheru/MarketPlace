using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Users.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : ICommand;