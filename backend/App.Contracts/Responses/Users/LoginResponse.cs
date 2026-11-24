namespace App.Contracts.Responses.Users;

// The refresh token is not part of the body: it is sent only as the HttpOnly cookie openkey_rt.
public record LoginResponse(string? AccessToken, bool MfaRequired, string? MfaToken = null);
