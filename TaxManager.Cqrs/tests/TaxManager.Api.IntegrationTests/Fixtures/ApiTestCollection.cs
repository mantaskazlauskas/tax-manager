namespace TaxManager.Api.IntegrationTests.Fixtures;

/// <summary>Shares one Postgres container across all integration test classes instead of one per class.</summary>
[CollectionDefinition(Name)]
public class ApiTestCollection : ICollectionFixture<TaxManagerApiFactory>
{
    public const string Name = "Api";
}
