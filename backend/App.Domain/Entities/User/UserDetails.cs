using App.Domain.Common;

namespace App.Domain.Entities;

public class UserDetails : PublicEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateTime? DateOfBirth { get; private set; }
    public string? PhoneNumber { get; private set; }
    public long UserId { get; private set; }
}