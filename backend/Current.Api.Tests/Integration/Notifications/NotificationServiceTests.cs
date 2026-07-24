using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Notifications;

public class NotificationServiceTests : IntegrationTestBase
{
    public NotificationServiceTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateNotificationAsync_EmptyTitle_Throws()
    {
        using var scope = Factory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var user = await SeedUserAsync(scope);

        var createNotificationTask = notificationService.CreateNotificationAsync(
            user.Id,
            NotificationType.System,
            "   ",
            "Valid body");

        await Assert.ThrowsAsync<InvalidOperationException>(() => createNotificationTask);
    }

    [Fact]
    public async Task TryCreateNotificationAsync_InvalidTitle_DoesNotThrow()
    {
        using var scope = Factory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var user = await SeedUserAsync(scope);

        var exception = await Record.ExceptionAsync(() =>
            notificationService.TryCreateNotificationAsync(
                user.Id,
                NotificationType.System,
                string.Empty,
                "Valid body"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task GetNotifications_OrdersByCreatedAtDescending()
    {
        using var scope = Factory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await SeedUserAsync(scope);
        var olderCreatedAt = DateTime.UtcNow.AddHours(-2);
        var newerCreatedAt = DateTime.UtcNow.AddHours(-1);

        await TestDataSeeder.SeedNotificationAsync(
            dbContext,
            user.Id,
            NotificationType.System,
            "Older notification",
            "Older body",
            createdAt: olderCreatedAt);

        await TestDataSeeder.SeedNotificationAsync(
            dbContext,
            user.Id,
            NotificationType.System,
            "Newer notification",
            "Newer body",
            createdAt: newerCreatedAt);

        var notifications = await notificationService.GetNotificationsAsync(user.Id);

        Assert.Equal(2, notifications.Count);
        Assert.Equal("Newer notification", notifications[0].Title);
        Assert.Equal("Older notification", notifications[1].Title);
    }

    private static async Task<User> SeedUserAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        return await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Notification",
            "User",
            $"notification-service-{Guid.NewGuid():N}@example.com",
            "Password123");
    }
}
