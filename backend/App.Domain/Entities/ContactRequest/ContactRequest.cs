using App.Domain.Common;
using App.Domain.Enums;

namespace App.Domain.Entities;

public class ContactRequest : PublicEntity
{
    private ContactRequest() { }

    public Guid AdvertUuid { get; private set; }
    public Guid FromUserUuid { get; private set; }
    public string Message { get; private set; } = null!;
    public ContactRequestStatus Status { get; private set; }

    public static ContactRequest Create(Guid advertUuid, Guid fromUserUuid, string message)
    {
        return new ContactRequest
        {
            AdvertUuid = advertUuid,
            FromUserUuid = fromUserUuid,
            Message = message,
            Status = ContactRequestStatus.Unread
        };
    }

    public void MarkAsRead()
    {
        Status = ContactRequestStatus.Read;
    }
}