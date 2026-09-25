namespace App.Contracts.Responses.Adverts;

public record ImportAdvertsResponse(
    int ImportedCount,
    int FailedCount,
    List<string> Errors
);
