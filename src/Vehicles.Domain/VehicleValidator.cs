namespace Vehicles.Domain;

public static class VehicleValidator
{
    public const int MinYear = 1886; // first petrol car
    public const decimal MaxWeight = 99_999_999.99m; // fits decimal(10,2)

    public static IReadOnlyList<string> Validate(Vehicle vehicle, int currentYear)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(vehicle.OwnerName))
            errors.Add("Owner's name is required.");
        if (vehicle.ManufacturerId <= 0)
            errors.Add("Manufacturer is required.");
        if (vehicle.Year < MinYear || vehicle.Year > currentYear)
            errors.Add($"Year of manufacture must be between {MinYear} and {currentYear}.");
        if (vehicle.Weight <= 0)
            errors.Add("Weight must be greater than 0.");
        else if (vehicle.Weight > MaxWeight)
            errors.Add($"Weight cannot exceed {MaxWeight} kg.");
        if (decimal.Round(vehicle.Weight, 2) != vehicle.Weight)
            errors.Add("Weight can have at most two decimal places.");

        return errors;
    }
}
