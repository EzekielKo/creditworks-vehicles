namespace Vehicles.Domain;

public static class CategoryResolver
{
    /// <summary>
    /// Returns the category with the highest MinWeight that is less than or equal to the weight.
    /// Ranges are half-open [Min, NextMin), so a boundary weight belongs to exactly one category.
    /// </summary>
    public static Category Resolve(decimal weight, IEnumerable<Category> categories)
    {
        var match = categories
            .Where(c => c.MinWeight <= weight)
            .OrderByDescending(c => c.MinWeight)
            .FirstOrDefault();

        return match ?? throw new InvalidOperationException(
            $"No category covers a weight of {weight} kg. The category configuration is invalid.");
    }
}
