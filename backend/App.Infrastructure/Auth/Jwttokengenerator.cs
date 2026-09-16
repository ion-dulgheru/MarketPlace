using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using App.Application.Abstractions.JWT;
using App.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace App.Infrastructure.Auth;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public (string Token, string JwtId) GenerateToken(User user)
    {
        var certificatePath = configuration["Jwt:CertificatePath"]
            ?? throw new InvalidOperationException("Jwt:CertificatePath is not configured.");
        var certificatePassword = configuration["Jwt:CertificatePassword"]
            ?? throw new InvalidOperationException("Jwt:CertificatePassword is not configured.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "15");

        var jwtId = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, jwtId),
        };

var certificate = X509CertificateLoader.LoadPkcs12FromFile(certificatePath, certificatePassword);        var key = new X509SecurityKey(certificate);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, jwtId);
    }
}