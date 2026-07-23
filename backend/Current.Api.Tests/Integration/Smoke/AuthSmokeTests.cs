using System.Net;
using Current.Api.Tests.Infrastructure;

namespace Current.Api.Tests.Integration.Smoke;

public class AuthSmokeTests : IntegrationTestBase
{
    public AuthSmokeTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetUsersMe_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
