using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Application.UseCases.Admin.GetAdvertReports;
using App.Application.UseCases.Admin.DismissAdvertReport;

namespace App.WebApi.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController(ISender sender) : BaseController
{
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