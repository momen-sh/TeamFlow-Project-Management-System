import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProjectService } from '../../services/project.service';
import { ProjectDto, CreateProjectDto } from '../../../../core/models/project.model';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-project-edit',
  templateUrl: './project-edit.component.html',
  styleUrls: ['./project-edit.component.css']
})
export class ProjectEditComponent implements OnInit {

  project?: ProjectDto;
  loading = true;
  saving = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private projectService: ProjectService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.projectService.getProjectById(id).subscribe({
      next: (res: ProjectDto) => {
        this.project = res;
        this.loading = false;
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load project';
        this.loading = false;
      }
    });
  }

  updateProject(project: CreateProjectDto) {
    if (!this.project?.id) return;

    this.saving = true;
    this.error = '';

    this.projectService.updateProject(this.project.id, project).subscribe({
      next: () => {
        this.notificationService.success('Project updated successfully.');
        this.router.navigate(['/projects/details', this.project!.id]);
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to update project';
        this.saving = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/projects']);
  }
} 
