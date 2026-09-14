using App.Domain.Entities;

namespace App.Application.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}