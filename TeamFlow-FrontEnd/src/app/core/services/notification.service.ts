import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type NotificationType = 'success' | 'error';

export interface AppNotification {
  message: string;
  type: NotificationType;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly notificationSubject = new BehaviorSubject<AppNotification | null>(null);

  notification$ = this.notificationSubject.asObservable();

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error');
  }

  clear(): void {
    this.notificationSubject.next(null);
  }

  private show(message: string, type: NotificationType): void {
    this.notificationSubject.next({ message, type });
    window.setTimeout(() => this.clear(), 4500);
  }
}
