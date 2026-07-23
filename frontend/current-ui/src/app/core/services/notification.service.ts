import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { API_PATHS } from '../config/api-paths';
import { ApiService } from './api.service';
import { Notification, UnreadNotificationCount } from '../../shared/models/notifications/notification.model';
import {
  normalizeNotificationResponse,
  normalizeUnreadCountResponse,
} from '../../shared/utils/notification-response.utils';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  constructor(private apiService: ApiService) {}

  getNotifications(): Observable<Notification[]> {
    return this.apiService
      .get<Notification[]>(API_PATHS.notifications.list)
      .pipe(map((notifications) => notifications.map((notification) => normalizeNotificationResponse(notification))));
  }

  getUnreadCount(): Observable<UnreadNotificationCount> {
    return this.apiService
      .get<UnreadNotificationCount>(API_PATHS.notifications.unreadCount)
      .pipe(map((response) => normalizeUnreadCountResponse(response)));
  }

  markAsRead(notificationId: string): Observable<void> {
    return this.apiService.patch<void>(API_PATHS.notifications.markRead(notificationId));
  }

  markAllAsRead(): Observable<void> {
    return this.apiService.patch<void>(API_PATHS.notifications.markAllRead);
  }
}
