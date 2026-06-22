import { Component } from '@angular/core';
import { ProjectService } from '../../services/project.service';
import { Router } from '@angular/router';
import { CreateProjectDto } from '../../../../core/models/project.model';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-project-create',
  templateUrl: './project-create.component.html',
  styleUrls: ['./project-create.component.css']
})
export class ProjectCreateComponent {

  loading = false;
  error = '';

  constructor(
    private projectService: ProjectService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  createProject(project: CreateProjectDto) {
    this.loading = true;
    this.error = '';

    this.projectService.createProject(project).subscribe({
      next: () => {
        this.notificationService.success('Project created successfully.');
        this.router.navigate(['/projects']);
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to create project';
        this.loading = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/projects']);
  }
} 
