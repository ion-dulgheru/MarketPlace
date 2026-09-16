namespace App.Contracts.Responses.Adverts;

public record AddressResponse(
    string Country,
    string City,
    string Region,
    string StreetAddress,
    string StreetNumber
);
