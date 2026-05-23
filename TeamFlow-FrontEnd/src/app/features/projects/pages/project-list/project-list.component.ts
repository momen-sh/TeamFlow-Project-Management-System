import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProjectService } from '../../services/project.service';
import { ProjectDto } from '../../../../core/models/project.model';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AppRoles } from '../../../../core/models/app-roles';

@Component({
  selector: 'app-project-list',
  templateUrl: './project-list.component.html',
  styleUrls: ['./project-list.component.css']
})
export class ProjectListComponent implements OnInit {

  projects: ProjectDto[] = [];
  loading = true;
  error = '';

  constructor(
    private projectService: ProjectService,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadProjects();
  }

  loadProjects() {
    this.loading = true;
    this.error = '';

    this.projectService.getProjects().subscribe({
      next: (res: ProjectDto[]) => {
        this.projects = res;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load projects';
        this.loading = false;
      }
    });
  }

  createProject() {
    this.router.navigate(['/projects/create']);
  }

  openProject(id: number) {
    this.router.navigate(['/projects/details', id]);
  }

  editProject(id: number) {
    this.router.navigate(['/projects/edit', id]);
  }

  deleteProject(id: number) {
    if (!this.canManageProjects()) return;

    const confirmed = confirm('Deleting this project will permanently delete the project and ALL tasks related to it. Are you sure?');
    if (!confirmed) return;

    this.projectService.deleteProject(id).subscribe({
      next: () => {
        this.projects = this.projects.filter(p => p.id !== id);
        this.notificationService.success('Project deleted successfully.');
      },
      error: (err: any) => {
        this.notificationService.error(err?.message || 'Failed to delete project.');
      }
    });
  }

  canManageProjects(): boolean {
    const role = this.authService.getUserRole();
    return role === AppRoles.Admin || role === AppRoles.TeamLeader;
  }
}
