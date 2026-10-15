namespace Vehicles.Domain;

public class Vehicle
{
    public int Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public int Year { get; set; }

    /// <summary>Weight in kg, max two decimal places.</summary>
    public decimal Weight { get; set; }
}
