namespace App.Contracts.Requests.Adverts;

public record GetAdvertsRequest(
    int Page = 1,
    int PageSize = 20,
    bool Mine = false,
    string? SearchTerm = null,
    string? Type = null,
    string? BuildingType = null,
    string? City = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinSurfaceArea = null,
    decimal? MaxSurfaceArea = null,
    int? Rooms = null,
    string? SortBy = null,
    bool SortDescending = true
);