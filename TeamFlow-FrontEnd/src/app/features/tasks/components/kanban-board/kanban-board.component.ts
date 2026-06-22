import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { combineLatest, map } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ProjectDto } from '../../../../core/models/project.model';
import { ProjectService } from '../../../projects/services/project.service';
import { UserDto } from '../../../../core/models/user.model';
import { UserService } from '../../../users/services/users.service';
import { TaskDto, TaskPriority, TaskStatus, TaskType } from '../../../../core/models/task.model';
import { TaskService } from '../../services/task.service';
import { TaskPermissionService } from '../../services/task-permission.service';
import { AppRoles } from '../../../../core/models/app-roles';
import { write } from '@popperjs/core';

interface KanbanColumn {
  id: string;
  title: string;
  status: TaskStatus;
  tasks: TaskDto[];
}

@Component({
  selector: 'app-kanban-board',
  templateUrl: './kanban-board.component.html',
  styleUrls: ['./kanban-board.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KanbanBoardComponent implements OnInit {
  readonly priorities = [
    { label: 'All priorities', value: 'all' },
    { label: 'Low', value: TaskPriority.Low },
    { label: 'Medium', value: TaskPriority.Medium },
    { label: 'High', value: TaskPriority.High }
  ] as const;
  readonly types = [
    { label: 'All types', value: 'all' },
    { label: 'Task', value: TaskType.Task },
    { label: 'Bug', value: TaskType.Bug },
    { label: 'Feature', value: TaskType.Feature }
  ] as const;

  readonly loading$ = this.taskService.loading$;
  readonly filters$ = this.taskService.filters$;
  readonly columns$ = combineLatest([
    this.taskService.todoTasks$,
    this.taskService.inProgressTasks$,
    this.taskService.doneTasks$
  ]).pipe(
    map(([todo, inProgress, done]) => [
      { id: 'todoList', title: 'To Do', status: TaskStatus.ToDo, tasks: todo },
      { id: 'inProgressList', title: 'In Progress', status: TaskStatus.InProgress, tasks: inProgress },
      { id: 'doneList', title: 'Done', status: TaskStatus.Done, tasks: done }
    ])
  );

  error = '';
  connectedDropLists = ['todoList', 'inProgressList', 'doneList'];
  projects: ProjectDto[] = [];
  users: UserDto[] = [];
  private projectNames = new Map<number, string>();
  private projectMap = new Map<number, ProjectDto>();
  private userNames = new Map<number, string>();

  constructor(
    private taskService: TaskService,
    private router: Router,
    private authService: AuthService,
    private permissionService: TaskPermissionService,
    private notificationService: NotificationService,
    private projectService: ProjectService,
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.projectService.getProjects().subscribe({
      next: projects => {
        this.projects = projects;
        this.projectNames = new Map(projects.map(project => [project.id, project.name]));
        this.projectMap = new Map(projects.map(project => [project.id, project]));
        this.cdr.markForCheck();
      }
    });

    if (this.authService.isAdmin()) {
      this.userService.getUsers().subscribe({
        next: users => {
          this.users = users;
          this.userNames = new Map(users.map(user => [user.id, this.fullName(user)]));
          this.cdr.markForCheck();
        }
      });
    }

    this.taskService.loadTasks().subscribe({
      error: (err: Error) => {
        this.error = err.message || 'Failed to load tasks';
      }
    });
  }

  drop(event: CdkDragDrop<TaskDto[]>, targetStatus: TaskStatus): void {
    const task = event.item.data as TaskDto;

    if (!task || task.status === targetStatus) return;
    if (!this.canMoveTask(task)) {
      this.notificationService.error('You can only move tasks assigned to you unless you are an Admin or project Team Leader.');
      return;
    }

    this.taskService.updateTaskStatus(task.id, targetStatus).subscribe({
      next: () => this.notificationService.success('Task status updated.'),
      error: (err: Error) => this.notificationService.error(err.message || 'Status update failed.')
    });
  }

  createTask(): void {
    this.router.navigate(['/tasks/create']);
  }

  openDetails(id: number): void {
    this.router.navigate(['/tasks', id]);
  }

  editTask(task: TaskDto): void {
    if (!this.canEditTask(task)) return;
    this.router.navigate(['/tasks', task.id, 'edit']);
  }

  deleteTask(id: number): void {
    const task = this.taskService.currentTasks.find(item => item.id === id);
    if (!task || !this.canDeleteTask(task)) return;
    if (!confirm('Delete this task?')) return;

    this.taskService.deleteTask(id).subscribe({
      next: () => this.notificationService.success('Task deleted.'),
      error: (err: Error) => this.notificationService.error(err.message || 'Delete failed.')
    });
  }

  updateSearch(search: string): void {
    this.taskService.setFilters({ search });
  }

  updatePriority(priority: string): void {
    this.taskService.setFilters({
      priority: priority === 'all' ? 'all' : priority as TaskPriority
    });
  }

  updateProject(projectId: string): void {
    this.taskService.setFilters({
      projectId: projectId === 'all' ? 'all' : Number(projectId)
    });
  }

  updateType(type: string): void {
    this.taskService.setFilters({
      type: type === 'all' ? 'all' : type as TaskType
    });
  }

  selfAssignTask(task: TaskDto): void {
    if (!this.permissionService.canSelfAssign(task)) return;

    this.taskService.selfAssignTask(task.id).subscribe({
      next: () => this.notificationService.success('Task assigned to you.'),
      error: (err: Error) => this.notificationService.error(err.message || 'Self assignment failed.')
    });
  }

  trackTask(_: number, task: TaskDto): number {
    return task.id;
  }

  trackColumn(_: number, column: KanbanColumn): string {
    return column.id;
  }

  projectName(task: TaskDto): string {
    return task.projectName || this.projectNames.get(task.projectId) || 'Project unavailable';
  }

  assigneeName(task: TaskDto): string {
    console.log(task.assignedUserName)
    return task.assignedUserName || 'Unassigned';
  }

  canEditTask(task: TaskDto): boolean {
    const role = this.authService.getUserRole();
    return (role === AppRoles.Admin || role === AppRoles.TeamLeader) && this.canModifyTask(task);
  }

  canManageTasks(): boolean {
    return this.projects.length === 0
      ? this.permissionService.canCreateTask()
      : this.projects.some(project => this.permissionService.canCreateTask(project));
  }

  canModifyTask(task: TaskDto): boolean {
    return this.permissionService.canModifyTask(task, this.projectMap.get(task.projectId));
  }

  canMoveTask(task: TaskDto): boolean {
    return this.permissionService.canMoveTask(task, this.projectMap.get(task.projectId));
  }

  canDeleteTask(task: TaskDto): boolean {
    return this.permissionService.canDeleteTask(task, this.projectMap.get(task.projectId));
  }

  canSelfAssign(task: TaskDto): boolean {
    return this.permissionService.canSelfAssign(task);
  }

  roleLabel(): string {
    return this.permissionService.roleLabel();
  }

  private fullName(user: UserDto): string {
    return `${user.firstName} ${user.lastName}`.trim() || user.email;
  }
}
