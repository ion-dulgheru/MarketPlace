namespace App.Application.UseCases.Users.SignIn;

public record SignInResponse(string AccessToken, string RefreshToken);