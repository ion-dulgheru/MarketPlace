
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
using App.Application.UseCases.Adverts.GetAdvertById;
using App.Application.UseCases.Adverts.AddAdvertPhoto;
using App.Application.UseCases.Adverts.DeleteAdvertPhoto;
using App.Application.UseCases.Adverts.FavoriteAdvert;
using App.Application.UseCases.Adverts.GetFavoriteAdverts;
using App.Application.UseCases.Adverts.SendContactRequest;
using App.Application.UseCases.Adverts.GetAdvertContactRequests;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/adverts")]
public class AdvertsController(ISender sender) : BaseController
{
    [HttpPost]
    [SwaggerResponse(201, "Advert created.", typeof(CreateAdvertResponse))]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdvertRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new CreateAdvertCommand(request, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : CreatedAtAction(nameof(GetById), new { uuid = result.Value }, new CreateAdvertResponse(result.Value));
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

    [AllowAnonymous]
    [HttpGet("{uuid:guid}")]
    [SwaggerResponse(200, "Advert details.", typeof(AdvertResponse))]
    [SwaggerResponse(400, "Invalid identifier.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> GetById(Guid uuid, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAdvertByIdQuery(uuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
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

    [HttpPost("{uuid:guid}/photos")]
    [Consumes("multipart/form-data")]
    [SwaggerResponse(201, "Photo added.", typeof(AdvertPhotoResponse))]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> AddPhoto(
        Guid uuid,
        IFormFile file,
        [FromForm] bool isPrimary = false,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ErrorDetails("Advert.FileRequired", "A valid photo file is required."));
        }

        await using var stream = file.OpenReadStream();
        var command = new AddAdvertPhotoCommand(
            uuid,
            UserUuid,
            stream,
            file.FileName,
            file.Length,
            isPrimary);

        var result = await sender.Send(command, ct);

        return result.IsFailure
            ? HandleFailure(result)
            : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpDelete("{uuid:guid}/photos/{photoUuid:guid}")]
    [SwaggerResponse(204, "Photo deleted.")]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert or photo not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> DeletePhoto(
        Guid uuid,
        Guid photoUuid,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new DeleteAdvertPhotoCommand(uuid, photoUuid, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }
    [HttpPost("{uuid:guid}/contact-requests")]
    [SwaggerResponse(201, "Contact request sent.")]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found or not active.", typeof(ErrorDetails))]
    public async Task<IActionResult> SendContactRequest(
        Guid uuid,
        [FromBody] SendContactRequestRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new SendContactRequestCommand(uuid, request.Message, UserUuid),
            ct);

        return result.IsFailure
            ? HandleFailure(result)
            : StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{uuid:guid}/contact-requests")]
    [SwaggerResponse(200, "Contact requests for this advert.", typeof(IReadOnlyList<ContactRequestResponse>))]
    [SwaggerResponse(400, "Invalid identifier.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> GetContactRequests(Guid uuid, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAdvertContactRequestsQuery(uuid, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

    [HttpPost("{uuid:guid}/favorite")]
    [SwaggerResponse(204, "Advert favorite status updated.")]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(401, "Not signed in.", typeof(ErrorDetails))]
    [SwaggerResponse(404, "Advert not found.", typeof(ErrorDetails))]
    public async Task<IActionResult> SetFavorite(
        Guid uuid,
        [FromBody] SetFavoriteAdvertRequest request,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new FavoriteAdvertCommand(uuid, UserUuid, request.IsFavorite), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }

    [HttpGet("favorites")]
    [SwaggerResponse(200, "Favorite adverts.", typeof(GetAdvertsResponse))]
    [SwaggerResponse(400, "Invalid pagination parameters.", typeof(ErrorDetails))]
    [SwaggerResponse(401, "Not signed in.", typeof(ErrorDetails))]
    public async Task<IActionResult> GetFavorites(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetFavoriteAdvertsQuery(UserUuid, page, pageSize), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }
}

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }
}