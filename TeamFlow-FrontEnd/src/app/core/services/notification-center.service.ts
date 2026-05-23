import { Injectable, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subscription, catchError, interval, of, switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environments';
import { NotificationDto } from '../models/notification.model';
import { ApiService } from './api.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationCenterService implements OnDestroy {
  private readonly notificationsSubject = new BehaviorSubject<NotificationDto[]>([]);
  private readonly unreadCountSubject = new BehaviorSubject<number>(0);
  private connection?: signalR.HubConnection;
  private pollingSubscription?: Subscription;
  private started = false;

  readonly notifications$ = this.notificationsSubject.asObservable();
  readonly unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(
    private api: ApiService,
    private tokenService: TokenService
  ) {}

  start(): void {
    if (this.started || !this.tokenService.hasToken()) return;
    this.started = true;

    this.loadNotifications().subscribe();
    this.loadUnreadCount().subscribe();
    this.startSignalR();
    this.startPollingFallback();
  }

  stop(): void {
    this.started = false;
    this.connection?.stop();
    this.connection = undefined;
    this.pollingSubscription?.unsubscribe();
    this.pollingSubscription = undefined;
    this.notificationsSubject.next([]);
    this.unreadCountSubject.next(0);
  }

  loadNotifications(): Observable<NotificationDto[]> {
    return this.api.get<NotificationDto[]>('notifications').pipe(
      tap(notifications => this.notificationsSubject.next(notifications))
    );
  }

  loadUnreadCount(): Observable<number> {
    return this.api.get<number>('notifications/unread-count').pipe(
      tap(count => this.unreadCountSubject.next(count))
    );
  }

  markAsRead(notificationId: number): Observable<NotificationDto> {
    return this.api.patch<NotificationDto>(`notifications/${notificationId}/read`, {}).pipe(
      tap(notification => {
        this.notificationsSubject.next(
          this.notificationsSubject.value.map(item => item.id === notification.id ? notification : item)
        );
        this.loadUnreadCount().subscribe();
      })
    );
  }

  private startSignalR(): void {
    const hubUrl = environment.apiUrl.replace(/\/api\/?$/i, '/hubs/notifications');
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => this.tokenService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('notificationReceived', (notification: NotificationDto) => {
      this.notificationsSubject.next([notification, ...this.notificationsSubject.value].slice(0, 50));
    });

    this.connection.on('unreadCountChanged', (count: number) => {
      this.unreadCountSubject.next(count);
    });

    this.connection.start().catch(() => {
      this.loadNotifications().subscribe();
      this.loadUnreadCount().subscribe();
    });
  }

  private startPollingFallback(): void {
    this.pollingSubscription = interval(30000).pipe(
      switchMap(() => this.started ? this.loadNotifications().pipe(switchMap(() => this.loadUnreadCount())) : of(0)),
      catchError(() => of(0))
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.stop();
  }
}
