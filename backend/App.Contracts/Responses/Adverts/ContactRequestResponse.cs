namespace App.Contracts.Responses.Adverts;

public record ContactRequestResponse(
    Guid Uuid,
    Guid FromUserUuid,
    string Message,
    string Status,
    DateTime CreatedDate
);