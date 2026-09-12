namespace App.Domain.Entities;

public class UserDetails
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public int Age { get; private set; }
    public int PhoneNumber { get; private set; }
    public long UserId { get; private set; }
}