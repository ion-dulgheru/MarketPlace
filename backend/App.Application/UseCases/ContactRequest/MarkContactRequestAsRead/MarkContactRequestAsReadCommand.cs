using App.Application.Abstractions.Messaging;

namespace App.Application.UseCases.ContactRequests.MarkContactRequestAsRead;

public record MarkContactRequestAsReadCommand(Guid ContactRequestUuid, Guid UserUuid) : ICommand;