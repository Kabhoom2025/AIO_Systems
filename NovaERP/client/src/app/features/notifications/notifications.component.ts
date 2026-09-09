import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { Subscription } from 'rxjs';
import { NotificationApiService, NotificationDto } from '../../core/notification-api.service';
import { NotificationHubService } from '../../core/notification-hub.service';
import { NotificationService } from '../../core/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, ButtonModule, TagModule, ToastModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit, OnDestroy {
  notifications: NotificationDto[] = [];
  loading = false;

  private hubSubscription?: Subscription;

  constructor(
    private api: NotificationApiService,
    private hub: NotificationHubService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void {
    this.load();
    // Shell already opens the SignalR connection; subscribe here to prepend newly arrived notifications live.
    this.hubSubscription = this.hub.notification$.subscribe(notification => {
      this.notifications = [notification, ...this.notifications];
    });
  }

  ngOnDestroy(): void {
    this.hubSubscription?.unsubscribe();
  }

  load() {
    this.loading = true;
    this.api.getMine(100).subscribe({
      next: rows => { this.notifications = rows; this.loading = false; },
      error: err => {
        this.loading = false;
        this.notify.error(err.error?.message ?? 'Failed to load notifications.');
      }
    });
  }

  markRead(notification: NotificationDto) {
    if (notification.isRead) return;
    this.api.markRead(notification.id).subscribe({
      next: () => { notification.isRead = true; },
      error: err => this.notify.error(err.error?.message ?? 'Failed to mark notification as read.')
    });
  }

  markAllRead() {
    this.api.markAllRead().subscribe({
      next: () => {
        this.notifications.forEach(n => (n.isRead = true));
        this.notify.success('All notifications marked as read.');
      },
      error: err => this.notify.error(err.error?.message ?? 'Failed to mark all notifications as read.')
    });
  }

  severityFor(type: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (type?.toLowerCase()) {
      case 'success': return 'success';
      case 'warning': return 'warn';
      case 'error':   return 'danger';
      default:        return 'info';
    }
  }
}
