namespace App.Contracts.Responses.Adverts;

public record AdvertReportResponse(
    Guid Uuid,
    Guid AdvertUuid,
    string AdvertTitle,
    Guid ReporterUuid,
    string Reason,
    string? Description,
    DateTime CreatedDate);