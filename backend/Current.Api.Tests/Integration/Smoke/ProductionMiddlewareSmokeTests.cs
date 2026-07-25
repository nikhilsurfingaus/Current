using System.Net;
using Current.Api.Tests.Infrastructure;

namespace Current.Api.Tests.Integration.Smoke;

public class ProductionMiddlewareSmokeTests : IntegrationTestBase
{
    public ProductionMiddlewareSmokeTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetHealth_IncludesSecurityHeaders()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
    }
}
