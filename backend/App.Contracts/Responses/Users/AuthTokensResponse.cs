namespace App.Contracts.Responses.Users;

public record AuthTokensResponse(string AccessToken, string RefreshToken);