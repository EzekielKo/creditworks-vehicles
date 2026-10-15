using Microsoft.AspNetCore.Mvc;
using Vehicles.Application;

namespace Vehicles.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
public class VehiclesController(VehicleService vehicles) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<VehicleListItem>> List(
        [FromQuery] string? sortBy, [FromQuery] bool descending, CancellationToken ct) =>
        await vehicles.ListAsync(sortBy, descending, ct);

    [HttpPost]
    public async Task<ActionResult<VehicleListItem>> Create(VehicleRequest request, CancellationToken ct)
    {
        var created = await vehicles.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }
}
