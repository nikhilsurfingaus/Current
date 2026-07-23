namespace Current.Api.Tests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<CurrentApiWebApplicationFactory>, IAsyncLifetime
{
    protected IntegrationTestBase(CurrentApiWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateAnonymousClient();
    }

    protected CurrentApiWebApplicationFactory Factory { get; }

    protected HttpClient Client { get; }

    public Task InitializeAsync() => Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;
}
