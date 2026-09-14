
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

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/adverts")]
public class AdvertsController(ISender sender) : BaseController
{
    [HttpPost]
    [SwaggerResponse(201, "Advert created.", typeof(Guid))]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdvertRequest request,
        CancellationToken ct = default)
    {
        // 1. Pass command along with the secure UserUuid from the token
        var result = await sender.Send(new CreateAdvertCommand(request, UserUuid), ct);

        // 2. Return 201 Created on success, or map error on failure
        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetById), new { uuid = result.Value }, result.Value);
    }

    [AllowAnonymous]
    [HttpGet]
    [SwaggerResponse(200, "Active adverts.", typeof(GetAdvertsResponse))]
    [SwaggerResponse(400, "Invalid pagination parameters.", typeof(ErrorDetails))]
    public async Task<IActionResult> GetActive(
        [FromQuery] GetAdvertsRequest request,
        CancellationToken ct = default)
    {
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
    public async Task<IActionResult> GetById(Guid uuid, CancellationToken ct = default)
    {
        // Fetch endpoint placeholder
        return Ok();
    }
}