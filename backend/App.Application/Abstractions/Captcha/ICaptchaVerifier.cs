namespace App.Application.Abstractions.Captcha;

public interface ICaptchaVerifier
{
    Task<CaptchaVerificationResult> VerifyAsync(string token, CancellationToken ct = default);
}

public record CaptchaVerificationResult(bool Success, double? Score);