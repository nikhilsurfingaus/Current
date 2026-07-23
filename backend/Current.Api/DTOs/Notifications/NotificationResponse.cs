using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NotificationType NotificationType { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
