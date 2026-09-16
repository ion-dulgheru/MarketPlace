namespace App.Contracts.Responses.Adverts;

public record GetAdvertsResponse(
    IReadOnlyList<AdvertResponse> Items,
    int Page,
    int PageSize,
    int TotalCount = 0
);