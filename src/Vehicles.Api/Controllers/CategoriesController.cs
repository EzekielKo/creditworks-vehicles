using Microsoft.AspNetCore.Mvc;
using Vehicles.Application;

namespace Vehicles.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(CategoryService categories) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<CategoryDto>> List(CancellationToken ct) =>
        await categories.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryRequest request, CancellationToken ct)
    {
        var created = await categories.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:int}")]
    public async Task<CategoryDto> Update(int id, CategoryRequest request, CancellationToken ct) =>
        await categories.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await categories.DeleteAsync(id, ct);
        return NoContent();
    }
}
