using Microsoft.AspNetCore.Mvc;
using Vehicles.Application;

namespace Vehicles.Api.Controllers;

[ApiController]
[Route("api/manufacturers")]
public class ManufacturersController(ManufacturerService manufacturers) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<ManufacturerDto>> List(CancellationToken ct) =>
        await manufacturers.ListAsync(ct);
}
