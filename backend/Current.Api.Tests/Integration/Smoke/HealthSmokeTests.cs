using System.Net;
using Current.Api.Tests.Infrastructure;

namespace Current.Api.Tests.Integration.Smoke;

public class HealthSmokeTests : IntegrationTestBase
{
    public HealthSmokeTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
