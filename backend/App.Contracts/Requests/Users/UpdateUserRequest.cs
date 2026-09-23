namespace App.Contracts.Requests.Users;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    string? PhoneNumber);
