using App.Contracts.Requests.Adverts;
using App.Contracts.Responses.Adverts;
using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.AttachAdvertImages;

public record AttachAdvertImagesCommand(
    List<AttachAdvertImagesItemRequest> Items,
    Guid UserUuid
) : ICommand<AttachAdvertImagesResponse>;
