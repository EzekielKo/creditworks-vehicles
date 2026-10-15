using System.Net.Http.Json;
using Vehicles.Application;

namespace Vehicles.Tests.Integration;

[Collection(DatabaseCollection.Name)]
public class SmokeTests(ApiFactory factory)
{
    [Fact]
    public async Task Categories_endpoint_returns_the_three_default_categories()
    {
        await factory.ResetAsync();
        var client = factory.CreateClient();

        var categories = await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");

        Assert.NotNull(categories);
        Assert.Equal(["Light", "Medium", "Heavy"], categories.Select(c => c.Name));
    }
}
