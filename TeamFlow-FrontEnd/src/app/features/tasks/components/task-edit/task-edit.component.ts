import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NotificationService } from '../../../../core/services/notification.service';
import { TaskDto } from '../../../../core/models/task.model';
import { TaskFormSubmit } from '../task-form/task-form.component';
import { TaskAttachmentService } from '../../services/task-attachment.service';
import { TaskService } from '../../services/task.service';
import { TaskPermissionService } from '../../services/task-permission.service';

@Component({
  selector: 'app-task-edit',
  templateUrl: './task-edit.component.html',
  styleUrls: ['./task-edit.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskEditComponent implements OnInit {
  task?: TaskDto;
  loading = true;
  saving = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService,
    private attachmentService: TaskAttachmentService,
    private permissionService: TaskPermissionService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.taskService.getTaskById(id).subscribe({
      next: task => {
        this.task = task;
        this.loading = false;
        if (!this.permissionService.canModifyTask(task)) {
          this.error = 'You can only modify tasks assigned to you unless you are an Admin or a project Team Leader.';
        }
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load task';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  updateTask(payload: TaskFormSubmit): void {
    if (!this.task) return;
    if (!this.permissionService.canModifyTask(this.task)) {
      this.error = 'You do not have permission to update this task.';
      return;
    }

    this.saving = true;
    this.error = '';

    this.taskService.updateTask(this.task.id, payload.task).subscribe({
      next: updatedTask => {
        if (payload.files.length === 0) {
          this.finishUpdate();
          return;
        }

        this.attachmentService.uploadAttachments(updatedTask.id, payload.files).subscribe({
          next: () => this.finishUpdate(),
          error: (err: Error) => {
            this.error = err.message || 'Task updated, but attachments failed to upload';
            this.saving = false;
            this.cdr.markForCheck();
          }
        });
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to update task';
        this.saving = false;
        this.cdr.markForCheck();
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/tasks']);
  }

  private finishUpdate(): void {
    this.notificationService.success('Task updated.');
    this.router.navigate(['/tasks']);
  }
}
