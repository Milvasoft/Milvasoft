namespace Milvasoft.IntegrationTests.DataAccessTests;

[CollectionDefinition(nameof(UtcTrueDatabaseTestCollection))]
public class UtcTrueDatabaseTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

[CollectionDefinition(nameof(UtcFalseDatabaseTestCollection))]
public class UtcFalseDatabaseTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}