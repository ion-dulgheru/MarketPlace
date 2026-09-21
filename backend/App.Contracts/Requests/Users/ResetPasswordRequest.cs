namespace App.Contracts.Requests.Users;

public record ResetPasswordRequest(string Token, string NewPassword);