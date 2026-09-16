using App.Domain.Entities;

namespace App.Application.Abstractions.JWT;

public interface IJwtTokenGenerator
{
    (string Token, string JwtId) GenerateToken(User user);
    string GenerateRefreshToken();
}