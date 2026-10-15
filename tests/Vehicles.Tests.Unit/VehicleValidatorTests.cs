using Vehicles.Domain;

namespace Vehicles.Tests.Unit;

public class VehicleValidatorTests
{
    private const int CurrentYear = 2026;

    private static Vehicle Valid() => new()
    {
        OwnerName = "John Smith",
        ManufacturerId = 1,
        Year = 2019,
        Weight = 1850.75m,
    };

    private static IReadOnlyList<string> Validate(Vehicle v) => VehicleValidator.Validate(v, CurrentYear);

    [Fact]
    public void Valid_vehicle_has_no_errors() => Assert.Empty(Validate(Valid()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Owner_name_is_required(string name)
    {
        var v = Valid(); v.OwnerName = name;
        Assert.NotEmpty(Validate(v));
    }

    [Fact]
    public void Manufacturer_is_required()
    {
        var v = Valid(); v.ManufacturerId = 0;
        Assert.NotEmpty(Validate(v));
    }

    [Theory]
    [InlineData(1885, false)]
    [InlineData(1886, true)]
    [InlineData(2026, true)]
    [InlineData(2027, false)]
    public void Year_must_be_in_range(int year, bool valid)
    {
        var v = Valid(); v.Year = year;
        Assert.Equal(valid, Validate(v).Count == 0);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(0.01, true)]
    [InlineData(1850.75, true)]
    [InlineData(1850.755, false)]
    public void Weight_must_be_positive_with_max_two_decimals(double weight, bool valid)
    {
        var v = Valid(); v.Weight = (decimal)weight;
        Assert.Equal(valid, Validate(v).Count == 0);
    }
}
