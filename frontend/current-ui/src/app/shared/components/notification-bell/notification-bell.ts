import { Component, ElementRef, HostListener, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { NotificationService } from '../../../core/services/notification.service';
import { Notification } from '../../models/notifications/notification.model';
import { getNotificationRoute } from '../../utils/notification-route.utils';
import { formatNotificationRelativeTime } from '../../utils/notification-time.utils';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.scss',
})
export class NotificationBellComponent implements OnInit {
  private notificationService = inject(NotificationService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  panelOpen = signal(false);
  notifications = signal<Notification[]>([]);
  unreadCount = signal(0);
  notificationsLoading = signal(false);

  readonly formatNotificationRelativeTime = formatNotificationRelativeTime;

  ngOnInit(): void {
    this.refreshUnreadCount();
  }

  togglePanel(): void {
    const nextPanelOpen = !this.panelOpen();

    this.panelOpen.set(nextPanelOpen);

    if (nextPanelOpen) {
      this.loadNotifications();
    }
  }

  closePanel(): void {
    this.panelOpen.set(false);
  }

  onNotificationClick(notification: Notification, event: MouseEvent): void {
    event.stopPropagation();

    const route = getNotificationRoute(notification);
    this.closePanel();

    if (route) {
      void this.router.navigateByUrl(route, { onSameUrlNavigation: 'reload' });
    }

    if (notification.isRead) {
      return;
    }

    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        this.notifications.update((items) =>
          items.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item)),
        );
        this.refreshUnreadCount();
      },
    });
  }

  onMarkAllRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.update((items) => items.map((item) => ({ ...item, isRead: true })));
        this.unreadCount.set(0);
      },
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.panelOpen()) {
      return;
    }

    const clickedInside = this.elementRef.nativeElement.contains(event.target);

    if (!clickedInside) {
      this.closePanel();
    }
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    this.closePanel();
  }

  private loadNotifications(): void {
    this.notificationsLoading.set(true);

    this.notificationService.getNotifications().subscribe({
      next: (notifications) => {
        this.notifications.set(notifications);
        this.notificationsLoading.set(false);
        this.refreshUnreadCount();
      },
      error: () => {
        this.notificationsLoading.set(false);
      },
    });
  }

  private refreshUnreadCount(): void {
    this.notificationService.getUnreadCount().subscribe({
      next: (response) => this.unreadCount.set(response.count),
    });
  }
}
