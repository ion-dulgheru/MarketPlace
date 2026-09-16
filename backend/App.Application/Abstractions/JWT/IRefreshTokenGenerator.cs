namespace App.Application.Abstractions.JWT;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
    string Hash(string token);
}