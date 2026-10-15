namespace Vehicles.Domain;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Icon key (e.g. "feather"); the frontend maps it to an actual graphic.</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Inclusive lower bound in kg. The upper bound is not stored: it is the next
    /// category's MinWeight (exclusive). The highest category has no upper bound.
    /// </summary>
    public decimal MinWeight { get; set; }
}
