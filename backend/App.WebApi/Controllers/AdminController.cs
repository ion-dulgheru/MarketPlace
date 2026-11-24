using App.Application.UseCases.Adverts.ImportAdverts;
using App.Application.UseCases.Adverts.AttachAdvertImages;
using App.Application.UseCases.Admin.GetAdvertReports;
using App.Application.UseCases.Admin.DismissAdvertReport;
using App.Contracts.Requests.Adverts;
using App.Contracts.Responses.Adverts;
using App.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace App.WebApi.Controllers;

[Authorize(Policy = "AdminOnly")]
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

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAdvertReportsQuery(page, pageSize), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

    [HttpDelete("reports/{uuid:guid}")]
    public async Task<IActionResult> DismissReport(Guid uuid, CancellationToken ct = default)
    {
        var result = await sender.Send(new DismissAdvertReportCommand(uuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }
}