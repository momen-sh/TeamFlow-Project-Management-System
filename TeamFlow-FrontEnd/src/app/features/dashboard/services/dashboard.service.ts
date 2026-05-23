import { Injectable } from '@angular/core';
import { catchError, forkJoin, map, of } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { TaskDto, TaskStatus, TaskType } from '../../../core/models/task.model';

export interface DashboardTask {
  title: string;
  status: string;
  priority: string;
  projectName: string;
}

export interface DashboardComment {
  content: string;
  author: string;
  createdAt: string;
}

export interface DashboardSummary {
  totalProjects: number;
  totalTasks: number;
  statusCounts: {
    todo: number;
    inProgress: number;
    done: number;
  };
  typeCounts: {
    task: number;
    bug: number;
    feature: number;
  };
  assignmentCounts: {
    assigned: number;
    unassigned: number;
  };
  recentTasks: DashboardTask[];
  recentComments: DashboardComment[];
}

interface DashboardApiResponse {
  totalProjects?: number;
  totalTasks?: number;
  statusCounts?: Partial<DashboardSummary['statusCounts']>;
  taskStatusCounts?: Partial<DashboardSummary['statusCounts']>;
  tasksByStatus?: Partial<DashboardSummary['statusCounts']>;
  typeCounts?: Partial<DashboardSummary['typeCounts']>;
  taskTypeCounts?: Partial<DashboardSummary['typeCounts']>;
  tasksByType?: Partial<DashboardSummary['typeCounts']>;
  assignmentCounts?: Partial<DashboardSummary['assignmentCounts']>;
  assignedVsUnassigned?: Partial<DashboardSummary['assignmentCounts']>;
  assignedTasks?: number;
  unassignedTasks?: number;
  recentTasks?: DashboardTask[] | TaskDto[];
  recentComments?: DashboardComment[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private api: ApiService) {}

  getSummary() {
    return forkJoin({
      dashboard: this.api.get<DashboardApiResponse>('dashboard').pipe(catchError(() => of(null))),
      tasks: this.api.get<TaskDto[]>('tasks').pipe(catchError(() => of([] as TaskDto[])))
    }).pipe(
      map(({ dashboard, tasks }) => this.withTaskFallbacks(
        dashboard ? this.normalizeSummary(dashboard) : this.emptySummary(),
        tasks
      )),
      catchError(() => of(this.emptySummary()))
    );
  }

  private normalizeSummary(response: DashboardApiResponse): DashboardSummary {
    const raw = response as DashboardApiResponse & Record<string, unknown>;
    const statusSource = response.statusCounts ?? response.taskStatusCounts ?? response.tasksByStatus ?? (raw['StatusCounts'] as Partial<DashboardSummary['statusCounts']>) ?? {};
    const typeSource = response.typeCounts ?? response.taskTypeCounts ?? response.tasksByType ?? (raw['TypeCounts'] as Partial<DashboardSummary['typeCounts']>) ?? {};
    const assignmentSource = response.assignmentCounts ?? response.assignedVsUnassigned ?? (raw['AssignmentCounts'] as Partial<DashboardSummary['assignmentCounts']>) ?? {};

    return {
      totalProjects: response.totalProjects ?? (Number(raw['TotalProjects']) || 0),
      totalTasks: response.totalTasks ?? (Number(raw['TotalTasks']) || 0),
      statusCounts: {
        todo: this.count(statusSource as Record<string, unknown>, 'todo', 'toDo', 'ToDo', '0'),
        inProgress: this.count(statusSource as Record<string, unknown>, 'inProgress', 'InProgress', '1'),
        done: this.count(statusSource as Record<string, unknown>, 'done', 'Done', '2')
      },
      typeCounts: {
        task: this.count(typeSource as Record<string, unknown>, 'task', 'Task', '0'),
        bug: this.count(typeSource as Record<string, unknown>, 'bug', 'Bug', '1'),
        feature: this.count(typeSource as Record<string, unknown>, 'feature', 'Feature', '2')
      },
      assignmentCounts: {
        assigned: response.assignedTasks ?? (Number(raw['AssignedTasks']) || this.count(assignmentSource as Record<string, unknown>, 'assigned', 'Assigned')),
        unassigned: response.unassignedTasks ?? (Number(raw['UnassignedTasks']) || this.count(assignmentSource as Record<string, unknown>, 'unassigned', 'Unassigned'))
      },
      recentTasks: ((response.recentTasks ?? raw['RecentTasks'] ?? []) as Array<DashboardTask | TaskDto>).slice(0, 6).map(task => ({
        title: task.title,
        status: this.statusLabel((task as TaskDto).status ?? (task as DashboardTask).status),
        priority: this.priorityLabel((task as TaskDto).priority ?? (task as DashboardTask).priority),
        projectName: (task as TaskDto).projectName ?? (task as DashboardTask).projectName ?? 'Project unavailable'
      })),
      recentComments: ((response.recentComments ?? raw['RecentComments'] ?? []) as DashboardComment[]).slice(0, 5)
    };
  }

  private emptySummary(): DashboardSummary {
    return {
      totalProjects: 0,
      totalTasks: 0,
      statusCounts: { todo: 0, inProgress: 0, done: 0 },
      typeCounts: { task: 0, bug: 0, feature: 0 },
      assignmentCounts: { assigned: 0, unassigned: 0 },
      recentTasks: [],
      recentComments: []
    };
  }

  private withTaskFallbacks(summary: DashboardSummary, tasks: TaskDto[]): DashboardSummary {
    if (tasks.length === 0) return summary;

    const statusTotal = summary.statusCounts.todo + summary.statusCounts.inProgress + summary.statusCounts.done;
    const typeTotal = summary.typeCounts.task + summary.typeCounts.bug + summary.typeCounts.feature;
    const assignmentTotal = summary.assignmentCounts.assigned + summary.assignmentCounts.unassigned;

    return {
      ...summary,
      totalTasks: summary.totalTasks || tasks.length,
      statusCounts: statusTotal ? summary.statusCounts : {
        todo: tasks.filter(task => this.normalizeStatus(task.status) === TaskStatus.ToDo).length,
        inProgress: tasks.filter(task => this.normalizeStatus(task.status) === TaskStatus.InProgress).length,
        done: tasks.filter(task => this.normalizeStatus(task.status) === TaskStatus.Done).length
      },
      typeCounts: typeTotal ? summary.typeCounts : {
        task: tasks.filter(task => this.normalizeType(task.type) === TaskType.Task).length,
        bug: tasks.filter(task => this.normalizeType(task.type) === TaskType.Bug).length,
        feature: tasks.filter(task => this.normalizeType(task.type) === TaskType.Feature).length
      },
      assignmentCounts: assignmentTotal ? summary.assignmentCounts : {
        assigned: tasks.filter(task => !!task.assignedUserId).length,
        unassigned: tasks.filter(task => !task.assignedUserId).length
      }
    };
  }

  private count(source: Record<string, unknown>, ...keys: string[]): number {
    const key = keys.find(candidate => source[candidate] !== undefined);
    return key ? Number(source[key]) || 0 : 0;
  }

  private statusLabel(status: TaskStatus | string | number): string {
    if (this.normalizeStatus(status) === TaskStatus.ToDo) return 'To Do';
    if (this.normalizeStatus(status) === TaskStatus.InProgress) return 'In Progress';
    if (this.normalizeStatus(status) === TaskStatus.Done) return 'Done';
    return String(status || 'Unknown');
  }

  private priorityLabel(priority: string | number): string {
    if (priority === 'Low' || priority === 0 || priority === '0') return 'Low';
    if (priority === 'Medium' || priority === 1 || priority === '1') return 'Medium';
    if (priority === 'High' || priority === 2 || priority === '2') return 'High';
    return String(priority || 'Unknown');
  }

  private normalizeStatus(status: TaskStatus | string | number): TaskStatus | string | number {
    if (status === TaskStatus.ToDo || status === 'ToDo' || status === 0 || status === '0') return TaskStatus.ToDo;
    if (status === TaskStatus.InProgress || status === 'InProgress' || status === 1 || status === '1') return TaskStatus.InProgress;
    if (status === TaskStatus.Done || status === 'Done' || status === 2 || status === '2') return TaskStatus.Done;
    return status;
  }

  private normalizeType(type: TaskType | string | number | undefined): TaskType | string | number | undefined {
    if (type === TaskType.Task || type === 'Task' || type === 0 || type === '0') return TaskType.Task;
    if (type === TaskType.Bug || type === 'Bug' || type === 1 || type === '1') return TaskType.Bug;
    if (type === TaskType.Feature || type === 'Feature' || type === 2 || type === '2') return TaskType.Feature;
    return type;
  }
}
