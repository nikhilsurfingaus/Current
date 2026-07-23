import { NotificationType } from '../enums';

export interface Notification {
  id: string;
  title: string;
  body: string;
  notificationType: NotificationType;
  relatedEntityId?: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface UnreadNotificationCount {
  count: number;
}
