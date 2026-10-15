namespace Vehicles.Domain;

public static class CategoryConfigurationValidator
{
    /// <summary>Validates the complete set of categories. Returns error messages (empty = valid).</summary>
    public static IReadOnlyList<string> Validate(IReadOnlyCollection<Category> categories)
    {
        var errors = new List<string>();

        if (categories.Count == 0)
        {
            errors.Add("At least one category is required.");
            return errors;
        }

        foreach (var c in categories)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                errors.Add("Every category must have a name.");
            if (string.IsNullOrWhiteSpace(c.Icon))
                errors.Add($"Category '{c.Name}' must have an icon.");
            if (c.MinWeight < 0)
                errors.Add($"Category '{c.Name}' cannot start below 0 kg.");
            if (decimal.Round(c.MinWeight, 2) != c.MinWeight)
                errors.Add($"Category '{c.Name}' weight can have at most two decimal places.");
        }

        var duplicateNames = categories
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);
        foreach (var g in duplicateNames)
            errors.Add($"Category name '{g.Key}' is used more than once.");

        var duplicateStarts = categories.GroupBy(c => c.MinWeight).Where(g => g.Count() > 1);
        foreach (var g in duplicateStarts)
            errors.Add($"More than one category starts at {g.Key} kg (ranges would overlap).");

        if (categories.Min(c => c.MinWeight) != 0)
            errors.Add("The lowest category must start at 0 kg so every weight has a category.");

        return errors;
    }
}
