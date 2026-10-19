namespace Vehicles.Domain;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Icon key (e.g. "feather"); the frontend maps it to an actual graphic.
    public string Icon { get; set; } = string.Empty;

    // Inclusive lower bound in kg. The upper bound is not stored: it is the next
    // category's MinWeight (exclusive). The highest category has no upper bound.
    public decimal MinWeight { get; set; }
}
