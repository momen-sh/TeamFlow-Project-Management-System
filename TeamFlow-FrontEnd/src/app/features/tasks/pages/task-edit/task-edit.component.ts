import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskService } from '../../services/tasks.service';
import { TaskDto, CreateTaskDto, UpdateTaskDto } from '../../../../core/models/task.model';
import { TaskStatus } from '../../../../core/models/task.model';
import { TaskPriority } from '../../../../core/models/task.model';
import { TaskType } from '../../../../core/models/task.model';
@Component({
  selector: 'app-task-edit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './task-edit.component.html',
  styleUrls: ['./task-edit.component.css']
})
export class TaskEditComponent implements OnInit {

  private taskId = 0;

  public form: UpdateTaskDto = {
    title: '',
    description: '',
    status: TaskStatus.ToDo,
    priority: TaskPriority.Medium,
    type: TaskType.Task,
    projectId: 0,
  };

  loading = true;
  saving = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private taskService: TaskService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.taskId = Number(this.route.snapshot.paramMap.get('id'));

    this.taskService.getTaskById(this.taskId).subscribe({
      next: (res: TaskDto) => {
        this.form = {
          title: res.title,
          description: res.description,
          status: res.status,
          priority: res.priority,
          type: res.type,
          projectId: res.projectId,
          assignedUserId: res.assignedUserId
        };
        this.loading = false;
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load task';
        this.loading = false;
      }
    });
  }

  update() {
    if (!this.taskId) return;

    this.saving = true;
    this.error = '';

    this.taskService.updateTask(this.taskId, this.form).subscribe({
      next: () => {
        this.router.navigate(['/tasks', this.form.projectId]);
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to update task';
        this.saving = false;
      }
    });
  }
}
