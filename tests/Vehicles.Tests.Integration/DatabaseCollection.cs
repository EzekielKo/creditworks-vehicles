namespace Vehicles.Tests.Integration;

/// <summary>
/// All integration tests share one factory and one database, and run one at a time.
/// </summary>
[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Database";
}
