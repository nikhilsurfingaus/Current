using System.Net.Http.Headers;
using Current.Api.Data;
using Current.Api.DTOs.Auth;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Services.Email;
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
        var accessToken = TestAuthHelper.CreateAccessToken(user);

        authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return authenticatedClient;
    }

    public async Task<HttpClient> CreateAuthenticatedClientViaLoginAsync(string email, string password)
    {
        using var scope = CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var authResponse = await authService.LoginAsync(new LoginRequest
        {
            Email = email,
            Password = password,
        });

        var authenticatedClient = CreateAnonymousClient();
        authenticatedClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.Token);

        return authenticatedClient;
    }

    public string GetVerificationCode(string email)
    {
        var emailSender = Services.GetRequiredService<CapturingEmailSender>();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return emailSender.VerificationCodesByEmail.TryGetValue(normalizedEmail, out var verificationCode)
            ? verificationCode
            : string.Empty;
    }

    public async Task ResetDatabaseAsync()
    {
        Services.GetRequiredService<CapturingEmailSender>().Clear();

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
