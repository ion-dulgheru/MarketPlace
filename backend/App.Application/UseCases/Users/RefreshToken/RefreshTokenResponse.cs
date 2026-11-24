namespace App.Application.UseCases.Users.RefreshToken;

// Internal result: the controller puts RefreshToken in the cookie and returns only AccessToken.
public record RefreshTokenResponse(string AccessToken, string RefreshToken);
