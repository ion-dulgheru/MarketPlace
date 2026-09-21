using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using App.Application.UseCases.ContactRequests.MarkContactRequestAsRead;
using App.Application.UseCases.ContactRequests.GetReceivedContactRequests;
using App.Contracts.Responses;
using App.Contracts.Responses.Adverts;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/contact-requests")]
public class ContactRequestsController(ISender sender) : BaseController
{
    [HttpGet]
    [SwaggerResponse(200, "Received contact requests.", typeof(IReadOnlyList<ContactRequestResponse>))]
    [SwaggerResponse(401, "Not signed in.", typeof(ErrorDetails))]
    public async Task<IActionResult> GetReceived(CancellationToken ct = default)
    {
        var result = await sender.Send(new GetReceivedContactRequestsQuery(UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

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