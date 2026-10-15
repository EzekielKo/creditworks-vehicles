using System.Net;
using System.Net.Http.Json;
using Vehicles.Application;

namespace Vehicles.Tests.Integration;

internal static class TestHelpers
{
    public static async Task<VehicleListItem> CreateVehicleAsync(
        this HttpClient client, string owner, int manufacturerId, int year, decimal weight)
    {
        var response = await client.PostAsJsonAsync("/api/vehicles",
            new VehicleRequest(owner, manufacturerId, year, weight));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<VehicleListItem>())!;
    }

    public static async Task<List<VehicleListItem>> ListVehiclesAsync(
        this HttpClient client, string? sortBy = null, bool descending = false)
    {
        var url = $"/api/vehicles?sortBy={sortBy}&descending={descending.ToString().ToLowerInvariant()}";
        return (await client.GetFromJsonAsync<List<VehicleListItem>>(url))!;
    }

    public static async Task<List<CategoryDto>> ListCategoriesAsync(this HttpClient client) =>
        (await client.GetFromJsonAsync<List<CategoryDto>>("/api/categories"))!;

    public static Task<HttpResponseMessage> UpdateCategoryAsync(
        this HttpClient client, int id, string name, string icon, decimal minWeight) =>
        client.PutAsJsonAsync($"/api/categories/{id}", new CategoryRequest(name, icon, minWeight));

    public static Task<HttpResponseMessage> CreateCategoryAsync(
        this HttpClient client, string name, string icon, decimal minWeight) =>
        client.PostAsJsonAsync("/api/categories", new CategoryRequest(name, icon, minWeight));
}
