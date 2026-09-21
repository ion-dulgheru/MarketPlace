namespace App.Contracts.Requests.Users;

public record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? PhoneNumber,
    string CaptchaToken);