using Vehicles.Domain;

namespace Vehicles.Tests.Unit;

public class CategoryResolverTests
{
    private static List<Category> Defaults() =>
    [
        new() { Name = "Light", Icon = "feather", MinWeight = 0 },
        new() { Name = "Medium", Icon = "car", MinWeight = 500 },
        new() { Name = "Heavy", Icon = "truck", MinWeight = 2500 },
    ];

    [Theory]
    [InlineData(0.01, "Light")]
    [InlineData(499.99, "Light")]
    [InlineData(500.00, "Medium")]   // boundary belongs to the upper category only
    [InlineData(500.01, "Medium")]
    [InlineData(2499.99, "Medium")]
    [InlineData(2500.00, "Heavy")]
    [InlineData(99999.99, "Heavy")]
    public void Resolve_returns_expected_category(double weight, string expected)
    {
        var result = CategoryResolver.Resolve((decimal)weight, Defaults());
        Assert.Equal(expected, result.Name);
    }

    [Fact]
    public void Resolve_is_independent_of_list_order()
    {
        var shuffled = Defaults().AsEnumerable().Reverse().ToList();
        Assert.Equal("Medium", CategoryResolver.Resolve(500m, shuffled).Name);
    }

    [Fact]
    public void Changing_category_boundaries_changes_existing_vehicle_category()
    {
        const decimal weight = 2200m; // example from the assignment, section 6
        var categories = Defaults();
        Assert.Equal("Medium", CategoryResolver.Resolve(weight, categories).Name);

        categories.Single(c => c.Name == "Heavy").MinWeight = 2000;
        Assert.Equal("Heavy", CategoryResolver.Resolve(weight, categories).Name);
    }

    [Fact]
    public void Resolve_throws_when_no_category_covers_weight()
    {
        var gapped = new List<Category> { new() { Name = "Medium", Icon = "car", MinWeight = 500 } };
        Assert.Throws<InvalidOperationException>(() => CategoryResolver.Resolve(100m, gapped));
    }
}
