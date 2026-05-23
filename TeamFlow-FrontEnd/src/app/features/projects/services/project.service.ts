import { Injectable } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { AssignProjectMemberDto, CreateProjectDto, ProjectDto } from '../../../core/models/project.model';


@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  constructor(private api: ApiService) { }

  getProjects() {
    return this.api.get<ProjectDto[]>('projects');
  }

  getProjectById(id: number) {
    return this.api.get<ProjectDto>(`projects/${id}`);
  }

  createProject(data: CreateProjectDto) {
    return this.api.post<ProjectDto>('projects', data);
  }

  updateProject(id: number, data: CreateProjectDto) {
    return this.api.put<any>(`projects/${id}`, data);
  }

  deleteProject(id: number) {
    return this.api.delete<any>(`projects/${id}`);
  }

  assignMembers(projectId: number, userIds: number[]) {
    return this.api.post<ProjectDto>(
      `projects/${projectId}/assign-users`,
      { userIds }
    );
  }

  removeMember(projectId: number, userId: number) {
    return this.api.delete<void>(`projects/${projectId}/assign-users/${userId}`);
  }
}
