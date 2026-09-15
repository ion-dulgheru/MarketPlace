using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Application.Abstractions;
using App.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using App.Application.Abstractions.JWT;
namespace App.Infrastructure.Auth;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public string GenerateToken(User user)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expiryMinutes = int.Parse(configuration["Jwt:ExpiryMinutes"] ?? "15");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}