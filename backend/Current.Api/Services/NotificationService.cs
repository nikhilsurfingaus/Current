using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Notifications;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class NotificationService : INotificationService
{
    private const int MaxNotificationsReturned = 50;

    private readonly ApplicationDbContext _dbContext;

    public NotificationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateNotificationAsync(
        Guid userId,
        NotificationType notificationType,
        string title,
        string body,
        Guid? relatedEntityId = null)
    {
        var notificationTitle = title.Trim();
        var notificationBody = body.Trim();

        if (string.IsNullOrWhiteSpace(notificationTitle))
        {
            throw new InvalidOperationException("Notification title is required.");
        }

        if (string.IsNullOrWhiteSpace(notificationBody))
        {
            throw new InvalidOperationException("Notification body is required.");
        }

        if (notificationTitle.Length > 200)
        {
            throw new InvalidOperationException("Notification title must be 200 characters or fewer.");
        }

        if (notificationBody.Length > 1000)
        {
            throw new InvalidOperationException("Notification body must be 1000 characters or fewer.");
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = notificationTitle,
            Body = notificationBody,
            NotificationType = notificationType,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync();
    }

    public async Task TryCreateNotificationAsync(
        Guid userId,
        NotificationType notificationType,
        string title,
        string body,
        Guid? relatedEntityId = null)
    {
        try
        {
            await CreateNotificationAsync(userId, notificationType, title, body, relatedEntityId);
        }
        catch
        {
            // Notifications are best-effort and must not fail the primary operation.
        }
    }

    public async Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync(Guid currentUserId)
    {
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == currentUserId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(MaxNotificationsReturned)
            .ToListAsync();

        return notifications.Select(notification => notification.ToResponse()).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid currentUserId)
    {
        return await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(notification =>
                notification.UserId == currentUserId && !notification.IsRead);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid currentUserId)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(item =>
                item.Id == notificationId && item.UserId == currentUserId);

        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();
        }

        return true;
    }

    public async Task MarkAllAsReadAsync(Guid currentUserId)
    {
        var unreadNotifications = await _dbContext.Notifications
            .Where(notification => notification.UserId == currentUserId && !notification.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0)
        {
            return;
        }

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _dbContext.SaveChangesAsync();
    }
}
