using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.Adverts.FavoriteAdvert;

public record FavoriteAdvertCommand(Guid AdvertUuid, Guid UserUuid, bool IsFavorite) : ICommand;
