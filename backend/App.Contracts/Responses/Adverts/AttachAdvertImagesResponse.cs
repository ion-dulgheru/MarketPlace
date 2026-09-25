namespace App.Contracts.Responses.Adverts;

public record AttachAdvertImagesResponse(
    int ProcessedCount,
    int FailedCount,
    List<string> Errors
);
