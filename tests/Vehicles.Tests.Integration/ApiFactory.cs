using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vehicles.Infrastructure;

namespace Vehicles.Tests.Integration;

/// <summary>
/// Starts the real API in memory against a separate test database (VehiclesDb_Test).
/// The server and credentials come from the same user-secret the API uses.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "VehiclesDb_Test";
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _migrated;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            string baseConnectionString;
            using (var sp = services.BuildServiceProvider())
            {
                baseConnectionString = sp.GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException(
                        "ConnectionStrings:DefaultConnection is not set. " +
                        "Set it with 'dotnet user-secrets set' (see the README).");
            }

            var testConnectionString = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = TestDatabaseName,
            }.ConnectionString;

            // Remove the API's DbContext registration (including EF's options-configuration
            // entries) and register the same context pointing at the test database.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<VehiclesDbContext>)
                    || d.ServiceType == typeof(VehiclesDbContext)
                    || (d.ServiceType.IsGenericType
                        && d.ServiceType.GetGenericTypeDefinition().Name.StartsWith("IDbContextOptionsConfiguration")
                        && d.ServiceType.GenericTypeArguments[0] == typeof(VehiclesDbContext)))
                .ToList();
            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<VehiclesDbContext>(o => o.UseSqlServer(testConnectionString));
        });
    }

    /// <summary>
    /// Creates the test database from the migrations (once), then restores the three
    /// default categories and removes all vehicles.
    /// </summary>
    public async Task ResetAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VehiclesDbContext>();

            if (!_migrated)
            {
                await db.Database.MigrateAsync();
                _migrated = true;
            }

            await db.Database.ExecuteSqlRawAsync("""
                DELETE FROM Vehicles;
                DELETE FROM Categories;
                DBCC CHECKIDENT ('Categories', RESEED, 0);
                INSERT INTO Categories (Name, Icon, MinWeight)
                VALUES ('Light', 'feather', 0), ('Medium', 'car', 500), ('Heavy', 'truck', 2500);
                """);
        }
        finally
        {
            _initLock.Release();
        }
    }
}
