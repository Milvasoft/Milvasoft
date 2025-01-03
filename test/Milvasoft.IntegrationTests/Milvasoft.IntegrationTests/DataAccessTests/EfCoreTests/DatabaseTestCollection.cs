namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

[CollectionDefinition(nameof(UtcTrueDatabaseTestCollection))]
public class UtcTrueDatabaseTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

[CollectionDefinition(nameof(UtcFalseDatabaseTestCollection))]
public class UtcFalseDatabaseTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}