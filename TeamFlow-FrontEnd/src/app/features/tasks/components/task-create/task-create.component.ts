import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { TaskFormSubmit } from '../task-form/task-form.component';
import { TaskAttachmentService } from '../../services/task-attachment.service';
import { TaskService } from '../../services/task.service';

@Component({
  selector: 'app-task-create',
  templateUrl: './task-create.component.html',
  styleUrls: ['./task-create.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskCreateComponent {
  saving = false;
  error = '';

  constructor(
    private taskService: TaskService,
    private attachmentService: TaskAttachmentService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  createTask(payload: TaskFormSubmit): void {
    this.saving = true;
    this.error = '';

    this.taskService.createTask(payload.task).subscribe({
      next: task => {
        if (payload.files.length === 0) {
          this.finishCreate();
          return;
        }

        this.attachmentService.uploadAttachments(task.id, payload.files).subscribe({
          next: () => this.finishCreate(),
          error: (err: Error) => {
            this.error = err.message || 'Task created, but attachments failed to upload';
            this.saving = false;
          }
        });
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to create task';
        this.saving = false;
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/tasks']);
  }

  private finishCreate(): void {
    this.notificationService.success('Task created.');
    this.router.navigate(['/tasks']);
  }
}
