namespace App.Contracts.Requests.Adverts;

public record GetAdvertsRequest(
    int Page = 1,
    int PageSize = 20,
    bool Mine = false
);