namespace App.Domain.Entites;

public class UserDetails{
    public string FirstName {get; private set; } = default;
    public string LastName {get; private set; } = default;
    public int Age {get; private set; }
    public int PhoneNumber {get; private set; } = default;
    public long UserId {get; private set; }
}