namespace App.Contracts.Responses.Adverts;

public record CreateAdvertResponse(Guid Guid)
{
    public Guid Id => Guid;
}
