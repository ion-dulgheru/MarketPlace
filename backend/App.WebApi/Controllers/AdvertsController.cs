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

    [HttpGet("{uuid:guid}")]
    public async Task<IActionResult> GetById(Guid uuid, CancellationToken ct = default)
    {
        // Fetch endpoint placeholder
        return Ok();
    }
}