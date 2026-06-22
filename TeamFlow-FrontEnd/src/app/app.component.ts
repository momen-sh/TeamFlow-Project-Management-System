import { Component } from '@angular/core';
import { NotificationService } from './core/services/notification.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  notification$ = this.notificationService.notification$;

  constructor(private notificationService: NotificationService) {}
}
