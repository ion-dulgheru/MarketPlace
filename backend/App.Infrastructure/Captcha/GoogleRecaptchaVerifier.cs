using System.Text.Json;
using App.Application.Abstractions.Captcha;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Captcha;

public class GoogleRecaptchaVerifier(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GoogleRecaptchaVerifier> logger) : ICaptchaVerifier
{
    public async Task<CaptchaVerificationResult> VerifyAsync(string token, CancellationToken ct = default)
    {
        var secret = configuration["Recaptcha:SecretKey"] ?? configuration["Recaptcha--SecretKey"];

        if (string.IsNullOrWhiteSpace(secret))
        {
            logger.LogError("reCAPTCHA verification failed: 'Recaptcha:SecretKey' is not configured in backend settings!");
            return new CaptchaVerificationResult(false, null);
        }

        try
        {
            var postData = new Dictionary<string, string>
            {
                ["secret"] = secret,
                ["response"] = token
            };

            var response = await httpClient.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(postData),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("reCAPTCHA endpoint returned HTTP status {StatusCode}", response.StatusCode);
                return new CaptchaVerificationResult(false, null);
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var success = root.GetProperty("success").GetBoolean();
            double? score = root.TryGetProperty("score", out var scoreProp) ? scoreProp.GetDouble() : null;

            if (!success)
            {
                logger.LogWarning("reCAPTCHA verification failed from Google. Response: {Response}", json);
            }
            else
            {
                logger.LogInformation("reCAPTCHA verification succeeded. Score: {Score}", score);
            }

            return new CaptchaVerificationResult(success, score);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "reCAPTCHA verification threw an exception.");
            return new CaptchaVerificationResult(false, null);
        }
    }
}