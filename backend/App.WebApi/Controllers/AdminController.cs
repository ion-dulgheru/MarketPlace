using App.Application.UseCases.Adverts.ImportAdverts;
using App.Application.UseCases.Adverts.AttachAdvertImages;
using App.Contracts.Requests.Adverts;
using App.Contracts.Responses.Adverts;
using App.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/admin")]
public class AdminController(ISender sender, IHostEnvironment env) : BaseController
{
    [HttpPost("adverts/import")]
    [SwaggerResponse(200, "Import completed.", typeof(ImportAdvertsResponse))]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(403, "Not authorized.", typeof(ErrorDetails))]
    public async Task<IActionResult> ImportAdverts(
        [FromBody] List<ImportAdvertItemRequest> adverts,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ImportAdvertsCommand(adverts, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

    [HttpPost("adverts/attach-local-images")]
    [SwaggerResponse(200, "Images attached.", typeof(AttachAdvertImagesResponse))]
    [SwaggerResponse(400, "Validation failed.", typeof(ErrorDetails))]
    [SwaggerResponse(403, "Not authorized.", typeof(ErrorDetails))]
    public async Task<IActionResult> AttachLocalImages(
        [FromBody] List<AttachAdvertImagesItemRequest> items,
        CancellationToken ct = default)
    {
        if (!env.IsDevelopment())
        {
            return NotFound();
        }

        var result = await sender.Send(new AttachAdvertImagesCommand(items, UserUuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }
}
