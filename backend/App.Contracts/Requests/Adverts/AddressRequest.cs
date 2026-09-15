namespace App.Contracts.Requests.Adverts;

public record AddressRequest(
    string Country,
    string City,
    string Region,
    string StreetAddress,
    string StreetNumber
);
