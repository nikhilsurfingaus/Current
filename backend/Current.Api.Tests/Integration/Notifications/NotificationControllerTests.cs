using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Notifications;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Notifications;

public class NotificationControllerTests : IntegrationTestBase
{
    private const string DefaultPassword = "Password123";

    public NotificationControllerTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetNotifications_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_ReturnsOnlyCurrentUserNotifications()
    {
        var (currentUser, otherUser) = await SeedTwoUsersAsync();

        using (var scope = Factory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.System,
                "Your transfer completed",
                "Funds moved to savings");

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.PaymentReceived,
                "Payment received",
                "A$25.00 from Alex");

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                otherUser.Id,
                NotificationType.System,
                "Other user alert",
                "Should not appear");
        }

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            currentUser.Email,
            DefaultPassword);

        var response = await authenticatedClient.GetAsync("/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var notifications = await response.ReadJsonAsync<List<NotificationResponse>>();

        Assert.NotNull(notifications);
        Assert.Equal(2, notifications.Count);
        Assert.All(notifications, notification => Assert.DoesNotContain("Other user", notification.Title));
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsUnreadNotificationsOnly()
    {
        var (currentUser, _) = await SeedTwoUsersAsync();

        using (var scope = Factory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.System,
                "Unread one",
                "Body one");

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.System,
                "Unread two",
                "Body two");

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.Security,
                "Already read",
                "Body three",
                isRead: true);
        }

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            currentUser.Email,
            DefaultPassword);

        var response = await authenticatedClient.GetAsync("/notifications/unread-count");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var unreadCountResponse = await response.ReadJsonAsync<UnreadNotificationCountResponse>();

        Assert.NotNull(unreadCountResponse);
        Assert.Equal(2, unreadCountResponse.Count);
    }

    [Fact]
    public async Task MarkAsRead_ValidNotification_ReturnsNoContent()
    {
        var (currentUser, _) = await SeedTwoUsersAsync();
        Notification unreadNotification;

        using (var scope = Factory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            unreadNotification = await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.System,
                "Mark me read",
                "Unread body");
        }

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            currentUser.Email,
            DefaultPassword);

        var response = await authenticatedClient.PatchAsync(
            $"/notifications/{unreadNotification.Id}/read",
            null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var unreadCountResponse = await authenticatedClient.GetAsync("/notifications/unread-count");
        var unreadCount = await unreadCountResponse.ReadJsonAsync<UnreadNotificationCountResponse>();

        Assert.NotNull(unreadCount);
        Assert.Equal(0, unreadCount.Count);
    }

    [Fact]
    public async Task MarkAsRead_OtherUsersNotification_ReturnsNotFound()
    {
        var (currentUser, otherUser) = await SeedTwoUsersAsync();
        Notification otherUserNotification;

        using (var scope = Factory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            otherUserNotification = await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                otherUser.Id,
                NotificationType.System,
                "Private alert",
                "Not yours");
        }

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            currentUser.Email,
            DefaultPassword);

        var response = await authenticatedClient.PatchAsync(
            $"/notifications/{otherUserNotification.Id}/read",
            null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_MarksEveryUnreadNotification()
    {
        var (currentUser, _) = await SeedTwoUsersAsync();

        using (var scope = Factory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.System,
                "Unread one",
                "Body one");

            await TestDataSeeder.SeedNotificationAsync(
                dbContext,
                currentUser.Id,
                NotificationType.PaymentReceived,
                "Unread two",
                "Body two");
        }

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            currentUser.Email,
            DefaultPassword);

        var response = await authenticatedClient.PatchAsync("/notifications/read-all", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var unreadCountResponse = await authenticatedClient.GetAsync("/notifications/unread-count");
        var unreadCount = await unreadCountResponse.ReadJsonAsync<UnreadNotificationCountResponse>();

        Assert.NotNull(unreadCount);
        Assert.Equal(0, unreadCount.Count);
    }

    private async Task<(User CurrentUser, User OtherUser)> SeedTwoUsersAsync()
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var currentUser = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Current",
            "User",
            $"notifications-current-{Guid.NewGuid():N}@example.com",
            DefaultPassword);

        var otherUser = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Other",
            "User",
            $"notifications-other-{Guid.NewGuid():N}@example.com",
            DefaultPassword);

        return (currentUser, otherUser);
    }
}
