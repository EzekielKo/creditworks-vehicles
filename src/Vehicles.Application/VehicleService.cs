using Microsoft.EntityFrameworkCore;
using Vehicles.Domain;
using Vehicles.Infrastructure;

namespace Vehicles.Application;

public class VehicleService(VehiclesDbContext db)
{
    public async Task<IReadOnlyList<VehicleListItem>> ListAsync(
        string? sortBy, bool descending, CancellationToken ct = default)
    {
        var query = db.Vehicles.AsNoTracking().Include(v => v.Manufacturer).AsQueryable();
        query = ApplySort(query, sortBy, descending);

        var vehicles = await query.ToListAsync(ct);
        var categories = await db.Categories.AsNoTracking().ToListAsync(ct);

        return vehicles.Select(v =>
        {
            var category = CategoryResolver.Resolve(v.Weight, categories);
            return new VehicleListItem(
                v.Id, v.OwnerName, v.Manufacturer!.Name, v.Year, v.Weight, category.Name, category.Icon);
        }).ToList();
    }

    public async Task<VehicleListItem> CreateAsync(VehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = new Vehicle
        {
            OwnerName = request.OwnerName?.Trim() ?? string.Empty,
            ManufacturerId = request.ManufacturerId,
            Year = request.Year,
            Weight = request.Weight,
        };

        var errors = VehicleValidator.Validate(vehicle, DateTime.UtcNow.Year).ToList();

        if (vehicle.ManufacturerId > 0 &&
            !await db.Manufacturers.AnyAsync(m => m.Id == vehicle.ManufacturerId, ct))
        {
            errors.Add("The selected manufacturer does not exist.");
        }

        if (errors.Count > 0)
            throw new ValidationFailedException(errors);

        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);

        var items = await ListAsync(null, false, ct);
        return items.Single(i => i.Id == vehicle.Id);
    }

    // Whitelist: only these sort keys are accepted, never raw strings.
    private static IQueryable<Vehicle> ApplySort(IQueryable<Vehicle> query, string? sortBy, bool descending) =>
        (sortBy?.ToLowerInvariant()) switch
        {
            "owner" => descending ? query.OrderByDescending(v => v.OwnerName) : query.OrderBy(v => v.OwnerName),
            "manufacturer" => descending
                ? query.OrderByDescending(v => v.Manufacturer!.Name)
                : query.OrderBy(v => v.Manufacturer!.Name),
            "year" => descending ? query.OrderByDescending(v => v.Year) : query.OrderBy(v => v.Year),
            "weight" => descending ? query.OrderByDescending(v => v.Weight) : query.OrderBy(v => v.Weight),
            _ => query.OrderBy(v => v.Id),
        };
}
