using Vehicles.Domain;

namespace Vehicles.Tests.Unit;

public class CategoryConfigurationValidatorTests
{
    private static Category Cat(string name, decimal min, string icon = "x") =>
        new() { Name = name, Icon = icon, MinWeight = min };

    [Fact]
    public void Default_configuration_is_valid()
    {
        var errors = CategoryConfigurationValidator.Validate(
            [Cat("Light", 0), Cat("Medium", 500), Cat("Heavy", 2500)]);
        Assert.Empty(errors);
    }

    [Fact]
    public void Single_category_starting_at_zero_is_valid()
    {
        Assert.Empty(CategoryConfigurationValidator.Validate([Cat("All", 0)]));
    }

    [Fact]
    public void Empty_configuration_is_rejected()
    {
        Assert.NotEmpty(CategoryConfigurationValidator.Validate([]));
    }

    [Fact]
    public void Gap_at_the_start_is_rejected()
    {
        var errors = CategoryConfigurationValidator.Validate([Cat("Medium", 100), Cat("Heavy", 2500)]);
        Assert.Contains(errors, e => e.Contains("start at 0"));
    }

    [Fact]
    public void Two_categories_with_same_start_are_rejected_as_overlap()
    {
        var errors = CategoryConfigurationValidator.Validate(
            [Cat("Light", 0), Cat("Medium", 500), Cat("Heavy", 500)]);
        Assert.Contains(errors, e => e.Contains("overlap"));
    }

    [Fact]
    public void Duplicate_names_are_rejected_case_insensitively()
    {
        var errors = CategoryConfigurationValidator.Validate([Cat("Light", 0), Cat("light", 500)]);
        Assert.Contains(errors, e => e.Contains("more than once"));
    }

    [Theory]
    [InlineData("", "icon")]
    [InlineData("Name", "")]
    public void Missing_name_or_icon_is_rejected(string name, string icon)
    {
        Assert.NotEmpty(CategoryConfigurationValidator.Validate([Cat(name, 0, icon)]));
    }

    [Fact]
    public void Negative_start_is_rejected()
    {
        Assert.NotEmpty(CategoryConfigurationValidator.Validate([Cat("A", -1), Cat("B", 0)]));
    }

    [Fact]
    public void More_than_two_decimal_places_is_rejected()
    {
        Assert.NotEmpty(CategoryConfigurationValidator.Validate([Cat("A", 0), Cat("B", 500.005m)]));
    }
}
