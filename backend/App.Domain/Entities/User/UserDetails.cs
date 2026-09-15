using App.Domain.Common;

namespace App.Domain.Entities;

public class UserDetails : PublicEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateTime? DateOfBirth { get; private set; }
    public string? PhoneNumber { get; private set; }
    public long UserId { get; private set; }

    private UserDetails() { }

    public static UserDetails Create(
        long userId,
        string firstName,
        string lastName,
        DateTime? dateOfBirth = null,
        string? phoneNumber = null)
    {
        return new UserDetails
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber
        };
    }
}