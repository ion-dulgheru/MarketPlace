namespace App.Domain.ValueObjects;

public class Address
{
    public string Country { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Region { get; private set; } = null!;
    public string StreetAddress { get; private set; } = null!;
    public string StreetNumber { get; private set; } = null!;
}