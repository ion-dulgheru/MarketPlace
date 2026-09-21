using System.Text.Json;
using App.Application.Abstractions.Captcha;
using Microsoft.Extensions.Configuration;

namespace App.Infrastructure.Captcha;

public class GoogleRecaptchaVerifier(HttpClient httpClient, IConfiguration configuration) : ICaptchaVerifier
{
    public async Task<CaptchaVerificationResult> VerifyAsync(string token, CancellationToken ct = default)
    {
        var secret = configuration["Recaptcha:SecretKey"]!;

        try
        {
            var response = await httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                content: null,
                ct);

            if (!response.IsSuccessStatusCode)
                return new CaptchaVerificationResult(false, null);

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var success = root.GetProperty("success").GetBoolean();
            double? score = root.TryGetProperty("score", out var scoreProp) ? scoreProp.GetDouble() : null;

            return new CaptchaVerificationResult(success, score);
        }
        catch
        {
            return new CaptchaVerificationResult(false, null); // provider inaccesibil → respins, niciodată acceptat tacit
        }
    }
}