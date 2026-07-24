using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Auth;

public class AuthorizationTests : IntegrationTestBase
{
    private const string DefaultPassword = "Password123";

    public AuthorizationTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAccounts_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/accounts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAccountById_OtherUsersAccount_ReturnsNotFound()
    {
        await SeedUserAsync("owner@example.com");
        await SeedUserAsync("other@example.com");

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var owner = await dbContext.Users.SingleAsync(user => user.Email == "owner@example.com");

        var ownerAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            owner.Id,
            "Bills",
            AccountType.Everyday,
            1000m);

        var otherUserClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            "other@example.com",
            DefaultPassword);

        var response = await otherUserClient.GetAsync($"/accounts/{ownerAccount.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersMe_WithValidToken_ReturnsOk()
    {
        await SeedUserAsync("me@example.com");

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            "me@example.com",
            DefaultPassword);

        var response = await authenticatedClient.GetAsync("/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<User> SeedUserAsync(string email)
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
            DefaultPassword);
    }
}
