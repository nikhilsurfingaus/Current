using Current.Api.Common.Enums;
using Current.Api.DTOs.Notifications;

namespace Current.Api.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(
        Guid userId,
        NotificationType notificationType,
        string title,
        string body,
        Guid? relatedEntityId = null);

    Task TryCreateNotificationAsync(
        Guid userId,
        NotificationType notificationType,
        string title,
        string body,
        Guid? relatedEntityId = null);

    Task<IReadOnlyList<NotificationResponse>> GetNotificationsAsync(Guid currentUserId);

    Task<int> GetUnreadCountAsync(Guid currentUserId);

    Task<bool> MarkAsReadAsync(Guid notificationId, Guid currentUserId);

    Task MarkAllAsReadAsync(Guid currentUserId);
}
