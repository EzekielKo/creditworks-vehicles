using Microsoft.EntityFrameworkCore;
using Vehicles.Domain;
using Vehicles.Infrastructure;

namespace Vehicles.Application;

public class CategoryService(VehiclesDbContext db)
{
    public async Task<IReadOnlyList<CategoryDto>> ListAsync(CancellationToken ct = default)
    {
        var categories = await db.Categories.AsNoTracking().OrderBy(c => c.MinWeight).ToListAsync(ct);
        return ToDtos(categories);
    }

    public async Task<CategoryDto> CreateAsync(CategoryRequest request, CancellationToken ct = default)
    {
        var category = new Category
        {
            Name = request.Name?.Trim() ?? string.Empty,
            Icon = request.Icon?.Trim() ?? string.Empty,
            MinWeight = request.MinWeight,
        };

        var all = await db.Categories.ToListAsync(ct);
        all.Add(category);
        EnsureValid(all);

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(category.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryRequest request, CancellationToken ct = default)
    {
        var all = await db.Categories.ToListAsync(ct);
        var category = all.SingleOrDefault(c => c.Id == id)
            ?? throw new NotFoundException($"Category {id} was not found.");

        category.Name = request.Name?.Trim() ?? string.Empty;
        category.Icon = request.Icon?.Trim() ?? string.Empty;
        category.MinWeight = request.MinWeight;
        EnsureValid(all);

        await db.SaveChangesAsync(ct);
        return await ToDtoAsync(category.Id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var all = await db.Categories.ToListAsync(ct);
        var category = all.SingleOrDefault(c => c.Id == id)
            ?? throw new NotFoundException($"Category {id} was not found.");

        all.Remove(category);
        EnsureValid(all);

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);
    }

    private static void EnsureValid(IReadOnlyCollection<Category> categories)
    {
        var errors = CategoryConfigurationValidator.Validate(categories);
        if (errors.Count > 0)
            throw new ValidationFailedException(errors);
    }

    private async Task<CategoryDto> ToDtoAsync(int id, CancellationToken ct)
    {
        var all = await ListAsync(ct);
        return all.Single(c => c.Id == id);
    }

    /// <summary>Upper bound of each category is the next category's MinWeight (null for the last).</summary>
    private static List<CategoryDto> ToDtos(List<Category> sortedByMinWeight) =>
        sortedByMinWeight
            .Select((c, i) => new CategoryDto(
                c.Id,
                c.Name,
                c.Icon,
                c.MinWeight,
                i + 1 < sortedByMinWeight.Count ? sortedByMinWeight[i + 1].MinWeight : null))
            .ToList();
}
