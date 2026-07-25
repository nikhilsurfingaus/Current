using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Auth;
using Current.Api.Services.Email;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Auth;

public class RegisterTests : IntegrationTestBase
{
    public RegisterTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedWithoutToken()
    {
        var registerRequest = new RegisterRequest
        {
            FirstName = "Nikhil",
            LastName = "Naik",
            Email = "nikhil@example.com",
            Password = "Password123",
        };

        var response = await Client.PostJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var registerResponse = await response.ReadJsonAsync<RegisterResponse>();

        Assert.NotNull(registerResponse);
        Assert.Equal("nikhil@example.com", registerResponse.Email);
        Assert.True(registerResponse.VerificationExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Register_ThenVerifyEmail_ReturnsTokenAndWelcomeNotification()
    {
        var registerRequest = new RegisterRequest
        {
            FirstName = "Mirabel",
            LastName = "Suttcliffe",
            Email = "mirabel@example.com",
            Password = "Password123",
        };

        var registerResponse = await Client.PostJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var verificationCode = Factory.GetVerificationCode("mirabel@example.com");
        Assert.False(string.IsNullOrWhiteSpace(verificationCode));

        var verifyResponse = await Client.PostJsonAsync("/auth/verify-email", new VerifyEmailRequest
        {
            Email = "mirabel@example.com",
            Code = verificationCode,
        });

        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var authResponse = await verifyResponse.ReadJsonAsync<AuthResponse>();

        Assert.NotNull(authResponse);
        Assert.NotEqual(Guid.Empty, authResponse.UserId);
        Assert.Equal("mirabel@example.com", authResponse.Email);
        Assert.Equal(UserRole.User, authResponse.Role);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var welcomeNotificationExists = await dbContext.Notifications
            .AsNoTracking()
            .AnyAsync(notification =>
                notification.UserId == authResponse.UserId &&
                notification.NotificationType == NotificationType.Security &&
                notification.Title == "Welcome to Current");

        Assert.True(welcomeNotificationExists);
    }

    [Fact]
    public async Task Register_DuplicateVerifiedEmail_ReturnsConflict()
    {
        var registerRequest = new RegisterRequest
        {
            FirstName = "Nikhil",
            LastName = "Naik",
            Email = "duplicate@example.com",
            Password = "Password123",
        };

        var firstResponse = await Client.PostJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var verificationCode = Factory.GetVerificationCode("duplicate@example.com");
        var verifyResponse = await Client.PostJsonAsync("/auth/verify-email", new VerifyEmailRequest
        {
            Email = "duplicate@example.com",
            Code = verificationCode,
        });
        Assert.Equal(HttpStatusCode.OK, verifyResponse.StatusCode);

        var duplicateResponse = await Client.PostJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Register_ShortPassword_ReturnsBadRequest()
    {
        var registerRequest = new RegisterRequest
        {
            FirstName = "Nikhil",
            LastName = "Naik",
            Email = "short-password@example.com",
            Password = "short",
        };

        var response = await Client.PostJsonAsync("/auth/register", registerRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnverifiedEmail_ReturnsForbidden()
    {
        var registerRequest = new RegisterRequest
        {
            FirstName = "Pending",
            LastName = "User",
            Email = "pending@example.com",
            Password = "Password123",
        };

        var registerResponse = await Client.PostJsonAsync("/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await Client.PostJsonAsync("/auth/login", new LoginRequest
        {
            Email = "pending@example.com",
            Password = "Password123",
        });

        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }
}
