using Current.Api.DTOs.Notifications;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class NotificationMappings
{
    public static NotificationResponse ToResponse(this Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Title = notification.Title,
            Body = notification.Body,
            NotificationType = notification.NotificationType,
            RelatedEntityId = notification.RelatedEntityId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
        };
    }
}
