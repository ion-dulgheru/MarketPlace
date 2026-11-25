namespace App.Contracts.Requests.Users;

public record MfaLoginRequest(string MfaToken, string Code);

public record MfaCodeRequest(string Code);

public record MfaSetupRequest(string CurrentPassword);
