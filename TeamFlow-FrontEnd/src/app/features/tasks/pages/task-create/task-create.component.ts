import { Component, OnInit } from '@angular/core';
import { TaskService } from '../../services/tasks.service';
import { CreateTaskDto, TaskPriority, TaskStatus, TaskType } from '../../../../core/models/task.model';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-task-create',
  templateUrl: './task-create.component.html',
  styleUrls: ['./task-create.component.css']
})
export class TaskCreateComponent implements OnInit {

  form: CreateTaskDto = {
    title: '',
    description: '',
    status: TaskStatus.ToDo,
    priority: TaskPriority.Medium,
    projectId: 0,
    type: TaskType.Task
  };
  loading = false;
  error = '';

  constructor(
    private taskService: TaskService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const projectId = Number(this.route.snapshot.paramMap.get('projectId'));
    this.form.projectId = projectId;
  }

  create() {
    this.loading = true;
    this.error = '';

    this.taskService.createTask(this.form).subscribe({
      next: () => {
        this.router.navigate(['/tasks', this.form.projectId]);
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to create task';
        this.loading = false;
      }
    });
  }
}
