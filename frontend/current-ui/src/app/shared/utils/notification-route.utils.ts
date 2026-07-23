import { NotificationType } from '../models/enums';
import { Notification } from '../models/notifications/notification.model';

export function getNotificationRoute(notification: Notification): string | null {
  if (!notification.relatedEntityId) {
    return null;
  }

  switch (notification.notificationType) {
    case NotificationType.PaymentSent:
    case NotificationType.PaymentReceived:
      return `/payments/${notification.relatedEntityId}`;
    default:
      return null;
  }
}
