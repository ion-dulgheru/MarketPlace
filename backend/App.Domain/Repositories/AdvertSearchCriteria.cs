using App.Domain.Entities;

namespace App.Domain.Repositories;

public record AdvertSearchCriteria(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    AdvertType? Type = null,
    BuildingType? BuildingType = null,
    string? City = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinSurfaceArea = null,
    decimal? MaxSurfaceArea = null,
    int? Rooms = null,
    string? SortBy = null,
    bool SortDescending = true
);
