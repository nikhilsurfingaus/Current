import { NotificationType } from '../models/enums';
import { Notification, UnreadNotificationCount } from '../models/notifications/notification.model';

export function parseNotificationType(value: unknown): NotificationType {
  if (typeof value === 'number' && !Number.isNaN(value)) {
    if (value >= NotificationType.PaymentSent && value <= NotificationType.System) {
      return value;
    }
  }

  if (typeof value === 'string') {
    const normalizedType = value.trim();

    switch (normalizedType) {
      case 'PaymentSent':
      case '0':
        return NotificationType.PaymentSent;
      case 'PaymentReceived':
      case '1':
        return NotificationType.PaymentReceived;
      case 'GoalCompleted':
      case '2':
        return NotificationType.GoalCompleted;
      case 'GoalContribution':
      case '3':
        return NotificationType.GoalContribution;
      case 'AccountCreated':
      case '4':
        return NotificationType.AccountCreated;
      case 'Security':
      case '5':
        return NotificationType.Security;
      case 'System':
      case '6':
        return NotificationType.System;
      default:
        break;
    }
  }

  return NotificationType.System;
}

function coerceRelatedEntityId(
  notification: Notification & { RelatedEntityId?: string | null },
): string | null {
  const relatedEntityId = notification.relatedEntityId ?? notification.RelatedEntityId;

  if (relatedEntityId === null || relatedEntityId === undefined) {
    return null;
  }

  const normalizedRelatedEntityId = String(relatedEntityId).trim();

  return normalizedRelatedEntityId.length > 0 ? normalizedRelatedEntityId : null;
}

export function normalizeNotificationResponse(
  notification: Notification & { RelatedEntityId?: string | null },
): Notification {
  return {
    ...notification,
    notificationType: parseNotificationType(notification.notificationType),
    relatedEntityId: coerceRelatedEntityId(notification),
    isRead: Boolean(notification.isRead),
  };
}

export function normalizeUnreadCountResponse(response: UnreadNotificationCount): UnreadNotificationCount {
  return {
    count: typeof response.count === 'number' ? response.count : 0,
  };
}
