using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using App.Application.UseCases.ContactRequests.MarkContactRequestAsRead;
using App.Contracts.Responses;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/contact-requests")]
public class ContactRequestsController(ISender sender) : BaseController
{
    [HttpPut("{uuid:guid}/status")]
    [SwaggerResponse(204, "Contact request marked as read.")]
    [SwaggerResponse(404, "Contact request not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> MarkAsRead(Guid uuid, CancellationToken ct = default)
    {
        var result = await sender.Send(new MarkContactRequestAsReadCommand(uuid, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }
}