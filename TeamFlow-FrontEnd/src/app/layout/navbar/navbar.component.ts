import { Component, OnDestroy, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { Router } from '@angular/router';
import { NotificationCenterService } from '../../core/services/notification-center.service';
import { NotificationDto, NotificationType } from '../../core/models/notification.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit, OnDestroy {
  userEmail: string | null = null;
  userRole: string | null = null;
  notifications: NotificationDto[] = [];
  unreadCount = 0;
  panelOpen = false;
  private readonly subscriptions = new Subscription();

  constructor(
    private authService: AuthService,
    private router: Router,
    private notificationCenter: NotificationCenterService
  ) {}

  ngOnInit(): void {
    this.userEmail = this.authService.getUserEmail();
    this.userRole = this.authService.getUserRole();
    this.notificationCenter.start();

    this.subscriptions.add(this.notificationCenter.notifications$.subscribe(items => {
      this.notifications = items;
    }));

    this.subscriptions.add(this.notificationCenter.unreadCount$.subscribe(count => {
      this.unreadCount = count;
    }));
  }

  logout(): void {
    this.notificationCenter.stop();
    this.authService.logout();
  }

  toggleNotifications(): void {
    this.panelOpen = !this.panelOpen;
  }

  navigateToNotification(notification: NotificationDto): void {
    const taskId = notification.relatedEntityId;
    if (notification.type === NotificationType.TaskSentToQA && taskId) {
      this.router.navigate(['/tasks', taskId], { fragment: 'qa-test-cases' });
    } else if (taskId) {
      this.router.navigate(['/tasks', taskId]);
    }

    this.markAsRead(notification);
    this.panelOpen = false;
  }

  markAsRead(notification: NotificationDto): void {
    if (notification.isRead) return;
    this.notificationCenter.markAsRead(notification.id).subscribe();
  }

  unreadLabel(): string {
    return this.unreadCount > 9 ? '9+' : String(this.unreadCount);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
