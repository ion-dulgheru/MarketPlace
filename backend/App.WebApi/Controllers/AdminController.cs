using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using App.Application.UseCases.Admin.GetAdvertReports;
using App.Application.UseCases.Admin.DismissAdvertReport;
using App.Domain.Repositories;
using App.Domain.Enums;

namespace App.WebApi.Controllers;

[Authorize]
[Route("api/admin")]
public class AdminController(ISender sender, IUserRepository userRepository) : BaseController
{
    private async Task<bool> CheckIsAdminAsync(CancellationToken ct)
    {
        if (IsAdmin) return true;
        if (UserId is null) return false;

        var user = await userRepository.GetByUuidAsync(UserUuid, ct);
        return user?.Role == UserRole.Admin;
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!await CheckIsAdminAsync(ct))
        {
            return Forbid();
        }

        var result = await sender.Send(new GetAdvertReportsQuery(page, pageSize), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : Ok(result.Value);
    }

    [HttpDelete("reports/{uuid:guid}")]
    public async Task<IActionResult> DismissReport(Guid uuid, CancellationToken ct = default)
    {
        if (!await CheckIsAdminAsync(ct))
        {
            return Forbid();
        }

        var result = await sender.Send(new DismissAdvertReportCommand(uuid), ct);

        return result.IsFailure
            ? HandleFailure(result)
            : NoContent();
    }
}