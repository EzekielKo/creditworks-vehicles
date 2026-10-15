using Microsoft.EntityFrameworkCore;
using Vehicles.Infrastructure;

namespace Vehicles.Application;

public class ManufacturerService(VehiclesDbContext db)
{
    public async Task<IReadOnlyList<ManufacturerDto>> ListAsync(CancellationToken ct = default) =>
        await db.Manufacturers
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new ManufacturerDto(m.Id, m.Name))
            .ToListAsync(ct);
}
