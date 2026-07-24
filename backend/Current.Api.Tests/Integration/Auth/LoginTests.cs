using System.Net;
using Current.Api.Data;
using Current.Api.DTOs.Auth;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Auth;

public class LoginTests : IntegrationTestBase
{
    public LoginTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        await SeedUserAsync("login@example.com", "Password123");

        var loginRequest = new LoginRequest
        {
            Email = "login@example.com",
            Password = "Password123",
        };

        var response = await Client.PostJsonAsync("/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.ReadJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);
        Assert.Equal("login@example.com", authResponse.Email);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        await SeedUserAsync("wrong-password@example.com", "Password123");

        var loginRequest = new LoginRequest
        {
            Email = "wrong-password@example.com",
            Password = "WrongPassword123",
        };

        var response = await Client.PostJsonAsync("/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<User> SeedUserAsync(string email, string password)
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        return await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Test",
            "User",
            email,
            password);
    }
}
