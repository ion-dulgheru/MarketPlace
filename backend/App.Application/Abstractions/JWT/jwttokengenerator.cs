using App.Domain.Entities;

namespace App.Application.Abstractions.JWT;

public interface IJwtTokenGenerator
{
     string GenerateToken(User user);
}