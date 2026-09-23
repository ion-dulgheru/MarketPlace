using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace App.Infrastructure.Auth;

public static class JwtCertificateLoader
{
    public static X509Certificate2 Load(IConfiguration configuration)
    {
        var certificatePassword = configuration["Jwt:CertificatePassword"]
            ?? throw new InvalidOperationException("Jwt:CertificatePassword is not configured.");

        var certificateBase64 = configuration["Jwt:CertificateBase64"];
        if (!string.IsNullOrWhiteSpace(certificateBase64))
        {
            var certificateBytes = Convert.FromBase64String(certificateBase64);
            return X509CertificateLoader.LoadPkcs12(certificateBytes, certificatePassword);
        }

        var certificatePath = configuration["Jwt:CertificatePath"]
            ?? throw new InvalidOperationException("Jwt:CertificatePath or Jwt:CertificateBase64 must be configured.");
        return X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);
    }
}
