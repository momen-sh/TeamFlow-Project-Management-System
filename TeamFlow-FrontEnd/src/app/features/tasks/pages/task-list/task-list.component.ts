import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TaskService } from '../../services/task.service';
import { TaskDto, TaskStatus } from '../../../../core/models/task.model';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {

  tasks: TaskDto[] = [];
  projectId: number | null = null;
  loading = true;
  error = '';
  readonly statuses = [
    { label: 'To Do', value: TaskStatus.ToDo },
    { label: 'In Progress', value: TaskStatus.InProgress },
    { label: 'Done', value: TaskStatus.Done }
  ];

  constructor(
    private taskService: TaskService,
    private route: ActivatedRoute,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const projectId = this.route.snapshot.paramMap.get('projectId');
    this.projectId = projectId ? Number(projectId) : null;
    this.loadTasks();
  }

  loadTasks() {
    this.loading = true;
    this.error = '';

    const request = this.projectId
      ? this.taskService.getTasksByProject(this.projectId)
      : this.taskService.getTasks();

    request.subscribe({
      next: (res: TaskDto[]) => {
        this.tasks = res;
        this.loading = false;
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load tasks';
        this.loading = false;
      }
    });
  }

  delete(id?: number) {
    if (id == null) return;
    if (!this.canManageTasks()) return;

    this.taskService.deleteTask(id).subscribe(() => {
      this.tasks = this.tasks.filter(t => t.id !== id);
      this.notificationService.success('Task deleted successfully.');
    });
  }

  updateStatus(task: TaskDto, status: string) {
    const nextStatus = status as TaskStatus;

    this.taskService.updateTaskStatus(task.id, { status: nextStatus }).subscribe({
      next: updatedTask => {
        task.status = updatedTask?.status ?? nextStatus;
        this.notificationService.success('Task status updated.');
      }
    });
  }

  canManageTasks(): boolean {
    return this.authService.isAdmin();
  }
}
