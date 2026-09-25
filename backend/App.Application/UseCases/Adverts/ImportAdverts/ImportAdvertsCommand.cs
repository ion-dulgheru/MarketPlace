using App.Contracts.Requests.Adverts;
using App.Contracts.Responses.Adverts;
using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.ImportAdverts;

public record ImportAdvertsCommand(
    List<ImportAdvertItemRequest> Adverts,
    Guid UserUuid
) : ICommand<ImportAdvertsResponse>;
