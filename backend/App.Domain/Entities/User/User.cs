using App.Domain.Common;

namespace App.Domain.Entities;

public class User : PublicEntity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailVerification { get; private set; }
}