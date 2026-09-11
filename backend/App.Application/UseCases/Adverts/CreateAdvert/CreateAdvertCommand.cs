namespace App.Application.UseCases.Adverts.CreateAdvert;

public record CreateAdvertCommand(
    CreateAdvertRequest Request,
    Guid UserUuid
) : ICommand<Guid>;