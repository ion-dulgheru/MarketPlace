namespace App.Contracts.Requests.Users;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);