
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using App.Contracts.Requests.Adverts;
using App.Application.UseCases.Adverts.CreateAdvert;
using App.Application.UseCases.Adverts.GetActiveAdverts;
using App.Application.UseCases.Adverts.UpdateAdvertStatus;
using App.Application.UseCases.Adverts.DeleteAdvert;
using App.Contracts.Responses;
using App.Contracts.Responses.Adverts;
using App.Application.UseCases.Adverts.UpdateAdvert;
using App.Application.UseCases.Adverts.GetMyAdverts;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/adverts")]
public class AdvertsController(ISender sender) : BaseController
{
    [HttpPost]
    [SwaggerResponse(204, "Advert created.")]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdvertRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new CreateAdvertCommand(request, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }

    [AllowAnonymous]
    [HttpGet]
    [SwaggerResponse(200, "Adverts.", typeof(GetAdvertsResponse))]
    [SwaggerResponse(400, "Invalid pagination parameters.", typeof(ErrorDetails))]
    [SwaggerResponse(401, "Not signed in.", typeof(ErrorDetails))]
    public async Task<IActionResult> GetActive(
        [FromQuery] GetAdvertsRequest request,
        CancellationToken ct = default)
    {
        if (request.Mine)
        {
            if (UserId is null)
            {
                return Unauthorized();
            }

            var myResult = await sender.Send(new GetMyAdvertsCommand(request, UserUuid), ct);

            return myResult.IsFailure
                ? HandleFailure(myResult)
                : Ok(myResult.Value);
        }

        var result = await sender.Send(new GetActiveAdvertsCommand(request), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

    [HttpPatch("{uuid:guid}/status")]
    [SwaggerResponse(204, "Advert status updated.")]
    [SwaggerResponse(400, "Invalid status.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> UpdateStatus(
        Guid uuid,
        [FromBody] UpdateAdvertStatusRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new UpdateAdvertStatusCommand(uuid, request, UserUuid),
            ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }

    [HttpDelete("{uuid:guid}")]
    [SwaggerResponse(204, "Advert deleted.")]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> Delete(Guid uuid, CancellationToken ct = default)
    {
        var result = await sender.Send(
            new DeleteAdvertCommand(uuid, UserUuid),
            ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }

    [HttpGet("{uuid:guid}")]
    [SwaggerResponse(204, "No advert content is available yet.")]
    public async Task<IActionResult> GetById(Guid uuid, CancellationToken ct = default)
    {
        return NoContent();
    }

    [HttpPut("{uuid:guid}")]
    [SwaggerResponse(200, "Advert updated.")]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> Update(
        Guid uuid,
        [FromBody] UpdateAdvertRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new UpdateAdvertCommand(uuid, request, UserUuid),
            ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok();
    }

    
}