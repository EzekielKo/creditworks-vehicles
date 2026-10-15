using System.Net;
using System.Net.Http.Json;
using Vehicles.Application;

namespace Vehicles.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class CategoryAndVehicleTests(ApiFactory factory)
{
    // Seeded manufacturer ids: 1 Mazda, 2 Mercedes, 3 Honda, 4 Ferrari, 5 Toyota.
    // Seeded category ids after reset: 1 Light (0), 2 Medium (500), 3 Heavy (2500).

    private async Task<HttpClient> FreshClientAsync()
    {
        await factory.ResetAsync();
        return factory.CreateClient();
    }

    // ---------- Category determination and boundaries ----------

    [Fact]
    public async Task Vehicle_at_exactly_500kg_is_Medium()
    {
        var client = await FreshClientAsync();
        var created = await client.CreateVehicleAsync("John Smith", 1, 2019, 500.00m);
        Assert.Equal("Medium", created.CategoryName);
    }

    [Theory]
    [InlineData(499.99, "Light")]
    [InlineData(500.01, "Medium")]
    [InlineData(2499.99, "Medium")]
    [InlineData(2500.00, "Heavy")]
    public async Task Vehicles_near_boundaries_get_the_expected_category(double weight, string expected)
    {
        var client = await FreshClientAsync();
        var created = await client.CreateVehicleAsync("Owner", 1, 2020, (decimal)weight);
        Assert.Equal(expected, created.CategoryName);
    }

    // ---------- Category changes affect existing vehicles ----------

    [Fact]
    public async Task Changing_category_boundaries_changes_existing_vehicle_category()
    {
        var client = await FreshClientAsync();
        await client.CreateVehicleAsync("Jane Turei", 2, 2015, 2200m);
        Assert.Equal("Medium", (await client.ListVehiclesAsync()).Single().CategoryName);

        var response = await client.UpdateCategoryAsync(3, "Heavy", "truck", 2000m);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Heavy", (await client.ListVehiclesAsync()).Single().CategoryName);
    }

    // ---------- Gaps and overlaps ----------

    [Fact]
    public async Task Invalid_category_change_returns_400_and_leaves_data_unchanged()
    {
        var client = await FreshClientAsync();

        var response = await client.UpdateCategoryAsync(1, "Light", "feather", 100m);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var categories = await client.ListCategoriesAsync();
        Assert.Equal(0m, categories.First(c => c.Name == "Light").MinWeight);
    }

    [Fact]
    public async Task Two_categories_starting_at_the_same_weight_are_rejected()
    {
        var client = await FreshClientAsync();

        var response = await client.UpdateCategoryAsync(3, "Heavy", "truck", 500m);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(3, (await client.ListCategoriesAsync()).Count);
    }

    [Fact]
    public async Task Deleting_the_lowest_category_is_rejected()
    {
        var client = await FreshClientAsync();

        var response = await client.DeleteAsync("/api/categories/1");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(3, (await client.ListCategoriesAsync()).Count);
    }

    [Fact]
    public async Task Deleting_a_middle_category_lets_the_one_below_absorb_its_range()
    {
        var client = await FreshClientAsync();
        await client.CreateVehicleAsync("Owner", 1, 2020, 1000m); // Medium

        var response = await client.DeleteAsync("/api/categories/2");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var categories = await client.ListCategoriesAsync();
        Assert.Equal(2, categories.Count);
        Assert.Equal(2500m, categories.Single(c => c.Name == "Light").MaxWeight);
        Assert.Equal("Light", (await client.ListVehiclesAsync()).Single().CategoryName);
    }

    [Fact]
    public async Task Creating_a_category_inside_an_existing_range_splits_it_without_gaps()
    {
        var client = await FreshClientAsync();

        var response = await client.CreateCategoryAsync("Compact", "bike", 250m);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var categories = await client.ListCategoriesAsync();
        var light = categories.Single(c => c.Name == "Light");
        var compact = categories.Single(c => c.Name == "Compact");
        Assert.Equal(250m, light.MaxWeight);
        Assert.Equal(250m, compact.MinWeight);
        Assert.Equal(500m, compact.MaxWeight);
    }

    [Fact]
    public async Task Category_without_name_or_icon_is_rejected()
    {
        var client = await FreshClientAsync();

        var response = await client.CreateCategoryAsync("", "", 100m);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- Vehicle validation ----------

    [Fact]
    public async Task Invalid_vehicle_returns_400_with_error_messages()
    {
        var client = await FreshClientAsync();

        var response = await client.PostAsJsonAsync("/api/vehicles",
            new VehicleRequest("", 1, 1800, -5m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Owner", body);
        Assert.Contains("Weight", body);
        Assert.DoesNotContain("   at ", body); // no stack trace
    }

    [Fact]
    public async Task Vehicle_with_unknown_manufacturer_is_rejected()
    {
        var client = await FreshClientAsync();

        var response = await client.PostAsJsonAsync("/api/vehicles",
            new VehicleRequest("Owner", 999, 2020, 1000m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Vehicle_weight_with_three_decimals_is_rejected()
    {
        var client = await FreshClientAsync();

        var response = await client.PostAsJsonAsync("/api/vehicles",
            new VehicleRequest("Owner", 1, 2020, 1000.555m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---------- Missing records ----------

    [Fact]
    public async Task Updating_a_missing_category_returns_404()
    {
        var client = await FreshClientAsync();
        var response = await client.UpdateCategoryAsync(999, "X", "x", 100m);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_a_missing_category_returns_404()
    {
        var client = await FreshClientAsync();
        var response = await client.DeleteAsync("/api/categories/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Sorting ----------

    private async Task<HttpClient> SeedForSortingAsync()
    {
        var client = await FreshClientAsync();
        await client.CreateVehicleAsync("Bob", 3, 2010, 3000m);   // Honda
        await client.CreateVehicleAsync("alice", 1, 2022, 400m);  // Mazda
        await client.CreateVehicleAsync("Carol", 2, 2015, 1500m); // Mercedes
        return client;
    }

    [Theory]
    [InlineData("owner", false, new[] { "alice", "Bob", "Carol" })]
    [InlineData("owner", true, new[] { "Carol", "Bob", "alice" })]
    [InlineData("manufacturer", false, new[] { "Bob", "alice", "Carol" })]   // Honda, Mazda, Mercedes
    [InlineData("manufacturer", true, new[] { "Carol", "alice", "Bob" })]
    [InlineData("year", false, new[] { "Bob", "Carol", "alice" })]           // 2010, 2015, 2022
    [InlineData("year", true, new[] { "alice", "Carol", "Bob" })]
    [InlineData("weight", false, new[] { "alice", "Carol", "Bob" })]         // 400, 1500, 3000
    [InlineData("weight", true, new[] { "Bob", "Carol", "alice" })]
    public async Task Vehicle_list_sorts_by_each_key_in_both_directions(
        string sortBy, bool descending, string[] expectedOwners)
    {
        var client = await SeedForSortingAsync();

        var list = await client.ListVehiclesAsync(sortBy, descending);

        Assert.Equal(expectedOwners, list.Select(v => v.OwnerName));
    }
}
