using App.Domain.Shared;
using App.Domain.Errors;

namespace App.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private Address(
        string country,
        string city,
        string region,
        string streetAddress,
        string streetNumber)
    {
        Country = country;
        City = city;
        Region = region;
        StreetAddress = streetAddress;
        StreetNumber = streetNumber;
    }

    public string Country { get; private set; }
    public string City { get; private set; }
    public string Region { get; private set; }
    public string StreetAddress { get; private set; }
    public string StreetNumber { get; private set; }

    public static Result<Address> Create(
        string? country,
        string? city,
        string? region,
        string? streetAddress,
        string? streetNumber)
    {
        var values = new[] { country, city, region, streetAddress, streetNumber };

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            return Result.Failure<Address>(AddressErrors.RequiredField);
        }

        return Result.Success(new Address(
            country!.Trim(),
            city!.Trim(),
            region!.Trim(),
            streetAddress!.Trim(),
            streetNumber!.Trim()));
    }

    protected override IEnumerable<object?> GetEqualityComponents() =>
        [Country, City, Region, StreetAddress, StreetNumber];
}