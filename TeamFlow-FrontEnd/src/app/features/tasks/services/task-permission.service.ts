import { Injectable } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { AppRoles } from '../../../core/models/app-roles';
import { ProjectDto } from '../../../core/models/project.model';
import { TaskDto } from '../../../core/models/task.model';

@Injectable({
  providedIn: 'root'
})
export class TaskPermissionService {
  constructor(private authService: AuthService) {}

  canCreateTask(project?: ProjectDto): boolean {
    if (this.isAdmin()) return true;
    if (!this.isTeamLeader()) return false;
    return project ? this.isProjectLeader(project) : true;
  }

  canModifyTask(task: TaskDto, project?: ProjectDto): boolean {
    if (this.isAdmin()) return true;
    if (this.isTeamLeader() && project && this.isProjectLeader(project)) return true;
    return this.isAssignedToCurrentUser(task);
  }

  canMoveTask(task: TaskDto, project?: ProjectDto): boolean {
    return this.canModifyTask(task, project);
  }

  canDeleteTask(task: TaskDto, project?: ProjectDto): boolean {
    if (this.isAdmin()) return true;
    return this.isTeamLeader() && !!project && this.isProjectLeader(project);
  }

  canSelfAssign(task: TaskDto): boolean {
    return !task.assignedUserId && !!this.currentUserId();
  }

  canAssign(task?: TaskDto): boolean {
    if (this.isAdmin()) return true;
    if (this.isTeamLeader()) return true;
    if (!task) return false;
    return this.isAssignedToCurrentUser(task);
  }

  currentUserId(): number | null {
    const rawId = this.authService.getUserId();
    const id = rawId === null ? NaN : Number(rawId);
    return Number.isFinite(id) ? id : null;
  }

  roleLabel(): string {
    return this.authService.getUserRole() || AppRoles.Developer;
  }

  private isAssignedToCurrentUser(task: TaskDto): boolean {
    const userId = this.currentUserId();
    return !!userId && Number(task.assignedUserId) === userId;
  }

  private isProjectLeader(project: ProjectDto): boolean {
    const userId = this.currentUserId();
    if (!userId) return false;
    if (Number(project.ownerId) === userId) return true;
    if (project.memberIds?.some(id => Number(id) === userId)) return true;
    return !!project.members?.some(member => Number(member.userId) === userId);
  }

  private isAdmin(): boolean {
    return this.authService.getUserRole() === AppRoles.Admin;
  }

  private isTeamLeader(): boolean {
    return this.authService.getUserRole() === AppRoles.TeamLeader;
  }
}
