using System.Net.Http.Headers;
using Current.Api.Data;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Infrastructure;

public sealed class CurrentApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection sqliteConnection = new("DataSource=:memory:");

    public CurrentApiWebApplicationFactory()
    {
        sqliteConnection.Open();
    }

    public IServiceScope CreateScope() => Services.CreateScope();

    public HttpClient CreateAnonymousClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    public HttpClient CreateAuthenticatedClient(User user)
    {
        var authenticatedClient = CreateAnonymousClient();
        var configuration = Services.GetRequiredService<IConfiguration>();
        var accessToken = TestAuthHelper.CreateAccessToken(user, configuration);

        authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return authenticatedClient;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Testing",
                ["Jwt:Issuer"] = "Current.Api.Tests",
                ["Jwt:Audience"] = "Current.Tests",
                ["Jwt:Key"] = "TEST_JWT_SIGNING_KEY_32_CHARS_MIN!!",
                ["Jwt:ExpiryMinutes"] = "60",
                ["Branch:WelcomeCreditAmount"] = "0",
                ["Branch:WelcomeCreditMaxAccounts"] = "0",
                ["Branch:InitialTreasuryBalance"] = "10000000",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(sqliteConnection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            sqliteConnection.Dispose();
        }

        base.Dispose(disposing);
    }
}
