import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest, map, tap } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { environment } from '../../../../environments/environments';
import {
  CreateTaskDto,
  CreateQaTestCaseDto,
  CreateTaskWorkRecordDto,
  QaTestCaseDto,
  QaTestCaseStatus,
  SendTaskToQaDto,
  TaskAttachmentDto,
  TaskDto,
  TaskFilters,
  TaskPriority,
  TaskStatus,
  TaskWorkRecordDto,
  TaskType,
  UpdateTaskStatusDto
} from '../../../core/models/task.model';

interface CreateTaskApiDto {
  title: string;
  description?: string;
  status: number;
  priority: number;
  type: number;
  projectId: number;
  assignedUserId?: number | null;
}

interface UpdateTaskStatusApiDto {
  status: number;
}

const DEFAULT_FILTERS: TaskFilters = {
  search: '',
  priority: 'all',
  projectId: 'all',
  type: 'all'
};

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly tasksSubject = new BehaviorSubject<TaskDto[]>([]);
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  private readonly filtersSubject = new BehaviorSubject<TaskFilters>(DEFAULT_FILTERS);

  readonly tasks$ = this.tasksSubject.asObservable();
  readonly loading$ = this.loadingSubject.asObservable();
  readonly filters$ = this.filtersSubject.asObservable();
  readonly filteredTasks$ = combineLatest([this.tasks$, this.filters$]).pipe(
    map(([tasks, filters]) => this.applyFilters(tasks, filters))
  );
  readonly todoTasks$ = this.byStatus(TaskStatus.ToDo);
  readonly inProgressTasks$ = this.byStatus(TaskStatus.InProgress);
  readonly doneTasks$ = this.byStatus(TaskStatus.Done);

  constructor(private api: ApiService) { }

  loadTasks(): Observable<TaskDto[]> {
    this.loadingSubject.next(true);

    return this.getTasks().pipe(
      tap({
        next: tasks => {
          this.tasksSubject.next(tasks);
          this.loadingSubject.next(false);
        },
        error: () => this.loadingSubject.next(false)
      })
    );
  }

  getTasks(): Observable<TaskDto[]> {
    return this.api.get<TaskDto[]>('tasks').pipe(
      map(tasks => tasks.map(task => this.normalizeTask(task)))
    );
  }

  getTasksByProject(projectId: number): Observable<TaskDto[]> {
    return this.api.get<TaskDto[]>(`projects/${projectId}/tasks`).pipe(
      map(tasks => tasks.map(task => this.normalizeTask(task)))
    );
  }

  getTaskById(id: number): Observable<TaskDto> {
    return this.api.get<TaskDto>(`tasks/${id}`).pipe(
      map(task => this.normalizeTask(task))
    );
  }

  createTask(data: CreateTaskDto): Observable<TaskDto> {
    return this.api.post<TaskDto>('tasks', this.toCreateTaskApiDto(data)).pipe(
      map(task => this.normalizeTask(task)),
      tap(task => this.tasksSubject.next([...this.tasksSubject.value, task]))
    );
  }

  updateTask(id: number, data: CreateTaskDto): Observable<TaskDto> {
    return this.api.put<TaskDto>(`tasks/${id}`, this.toCreateTaskApiDto(data)).pipe(
      map(task => this.normalizeTask(task)),
      tap(task => this.replaceTask(id, task))
    );
  }

  deleteTask(id: number): Observable<void> {
    return this.api.delete<void>(`tasks/${id}`).pipe(
      tap(() => this.tasksSubject.next(this.tasksSubject.value.filter(task => task.id !== id)))
    );
  }

  updateTaskStatus(id: number, statusOrBody: TaskStatus | UpdateTaskStatusDto): Observable<TaskDto> {
    const status = typeof statusOrBody === 'object' ? statusOrBody.status : statusOrBody;
    const previousTasks = this.tasksSubject.value;
    const optimisticTasks = previousTasks.map(task =>
      task.id === id ? { ...task, status } : task
    );

    this.tasksSubject.next(optimisticTasks);

    const body: UpdateTaskStatusApiDto = { status: this.statusToNumber(status) };
    return this.api.patch<TaskDto>(`tasks/${id}/status`, body).pipe(
      map(task => this.normalizeTask(task)),
      tap({
        next: task => this.replaceTask(id, task),
        error: () => this.tasksSubject.next(previousTasks)
      })
    );
  }

  selfAssignTask(id: number): Observable<TaskDto> {
    return this.api.post<TaskDto>(`tasks/${id}/self-assign`, {}).pipe(
      map(task => this.normalizeTask(task)),
      tap(task => this.replaceTask(id, task))
    );
  }

  unassignTask(id: number): Observable<TaskDto> {
    return this.api.patch<TaskDto>(`tasks/${id}/unassign`, {}).pipe(
      map(task => this.normalizeTask(task)),
      tap(task => this.replaceTask(id, task))
    );
  }

  sendToQa(id: number, data: SendTaskToQaDto): Observable<TaskDto> {
    return this.api.post<TaskDto>(`tasks/${id}/send-to-qa`, data).pipe(
      map(task => this.normalizeTask(task)),
      tap(task => this.replaceTask(id, task))
    );
  }

  getWorkRecords(taskId: number): Observable<TaskWorkRecordDto[]> {
    return this.api.get<TaskWorkRecordDto[]>(`tasks/${taskId}/work-records`);
  }

  createWorkRecord(taskId: number, data: CreateTaskWorkRecordDto): Observable<TaskWorkRecordDto> {
    return this.api.post<TaskWorkRecordDto>(`tasks/${taskId}/work-records`, data);
  }

  getQaTestCases(taskId: number): Observable<QaTestCaseDto[]> {
    return this.api.get<QaTestCaseDto[]>(`tasks/${taskId}/qa-test-cases`).pipe(
      map(testCases => testCases.map(testCase => this.normalizeQaTestCase(testCase)))
    );
  }

  createQaTestCase(taskId: number, data: CreateQaTestCaseDto): Observable<QaTestCaseDto> {
    return this.api.post<QaTestCaseDto>(`tasks/${taskId}/qa-test-cases`, {
      ...data,
      status: this.qaStatusToNumber(data.status)
    }).pipe(
      map(testCase => this.normalizeQaTestCase(testCase))
    );
  }

  updateQaTestCaseStatus(taskId: number, testCaseId: number, status: QaTestCaseStatus): Observable<QaTestCaseDto> {
    return this.api.patch<QaTestCaseDto>(`tasks/${taskId}/qa-test-cases/${testCaseId}/status`, {
      status: this.qaStatusToNumber(status)
    }).pipe(
      map(testCase => this.normalizeQaTestCase(testCase))
    );
  }

  setFilters(filters: Partial<TaskFilters>): void {
    this.filtersSubject.next({
      ...this.filtersSubject.value,
      ...filters
    });
  }

  get currentTasks(): TaskDto[] {
    return this.tasksSubject.value;
  }

  private replaceTask(id: number, updatedTask: TaskDto): void {
    this.tasksSubject.next(
      this.tasksSubject.value.map(task => task.id === id ? updatedTask : task)
    );
  }

  private byStatus(status: TaskStatus): Observable<TaskDto[]> {
    return this.filteredTasks$.pipe(
      map(tasks => tasks.filter(task => this.normalizeStatus(task.status) === status))
    );
  }

  private applyFilters(tasks: TaskDto[], filters: TaskFilters): TaskDto[] {
    const search = filters.search.trim().toLowerCase();

    return tasks.filter(task => {
      const matchesSearch = !search || task.title.toLowerCase().includes(search);
      const matchesPriority = filters.priority === 'all' || this.normalizePriority(task.priority) === filters.priority;
      const matchesProject = filters.projectId === 'all' || Number(task.projectId) === filters.projectId;
      const matchesType = filters.type === 'all' || this.normalizeType(task.type ?? TaskType.Task) === filters.type;

      return matchesSearch && matchesPriority && matchesProject && matchesType;
    });
  }

  private normalizeStatus(status: TaskStatus | number | string): TaskStatus {
    if (status === 0 || status === '0') return TaskStatus.ToDo;
    if (status === 1 || status === '1') return TaskStatus.InProgress;
    if (status === 2 || status === '2') return TaskStatus.Done;
    return status as TaskStatus;
  }

  private normalizePriority(priority: TaskPriority | number | string): TaskPriority {
    if (priority === 0 || priority === '0') return TaskPriority.Low;
    if (priority === 1 || priority === '1') return TaskPriority.Medium;
    if (priority === 2 || priority === '2') return TaskPriority.High;
    return priority as TaskPriority;
  }

  private normalizeType(type: TaskType | number | string): TaskType {
    if (type === 0 || type === '0') return TaskType.Task;
    if (type === 1 || type === '1') return TaskType.Bug;
    if (type === 2 || type === '2') return TaskType.Feature;
    return type as TaskType;
  }

  private normalizeTask(task: TaskDto): TaskDto {
    return {
      ...task,
      status: this.normalizeStatus(task.status),
      priority: this.normalizePriority(task.priority),
      type: this.normalizeType(task.type ?? TaskType.Task),
      assignedUserId: task.assignedUserId ?? null,
      sentToQaAt: task.sentToQaAt ?? null,
      sentToQaByUserId: task.sentToQaByUserId ?? null,
      attachments: task.attachments?.map(attachment => this.normalizeAttachment(attachment)) ?? [],
      workRecords: task.workRecords ?? [],
      qaTestCases: task.qaTestCases?.map(testCase => this.normalizeQaTestCase(testCase)) ?? [],
      qaAssignments: task.qaAssignments ?? [],
      permissions: task.permissions ?? {
        canManage: false,
        canAddWorkRecord: false,
        canSendToQa: false,
        canAddQaTestCase: false,
        canComment: true,
        canUnassign: false
      }
    };
  }

  private normalizeQaTestCase(testCase: QaTestCaseDto): QaTestCaseDto {
    return {
      ...testCase,
      status: this.normalizeQaStatus(testCase.status)
    };
  }

  private normalizeAttachment(attachment: TaskAttachmentDto): TaskAttachmentDto {
    const raw = attachment as TaskAttachmentDto & Record<string, unknown>;
    const url = attachment.url ?? attachment.fileUrl ?? raw['FileUrl'] as string ?? '';
    return {
      ...attachment,
      fileName: attachment.fileName ?? raw['FileName'] as string ?? '',
      contentType: attachment.contentType ?? attachment.fileType ?? raw['FileType'] as string ?? '',
      fileType: attachment.fileType ?? attachment.contentType ?? raw['FileType'] as string ?? '',
      size: attachment.size ?? attachment.fileSize ?? (Number(raw['FileSize']) || 0),
      fileSize: attachment.fileSize ?? attachment.size ?? (Number(raw['FileSize']) || 0),
      url: this.absoluteFileUrl(url),
      fileUrl: this.absoluteFileUrl(url),
      createdAt: attachment.createdAt ?? raw['CreatedAt'] as string,
      uploadedAt: attachment.uploadedAt ?? attachment.createdAt ?? raw['CreatedAt'] as string
    };
  }

  private absoluteFileUrl(url: string): string {
    if (!url) return '';
    if (/^https?:\/\//i.test(url)) return url;
    const apiOrigin = environment.apiUrl.replace(/\/api\/?$/i, '');
    return `${apiOrigin}${url.startsWith('/') ? url : `/${url}`}`;
  }

  private toCreateTaskApiDto(data: CreateTaskDto): CreateTaskApiDto {
    return {
      title: data.title,
      description: data.description,
      status: this.statusToNumber(data.status),
      priority: this.priorityToNumber(data.priority),
      type: this.typeToNumber(data.type),
      projectId: data.projectId,
      assignedUserId: data.assignedUserId ?? null
    };
  }

  private statusToNumber(status: TaskStatus | number | string): number {
    return {
      [TaskStatus.ToDo]: 0,
      [TaskStatus.InProgress]: 1,
      [TaskStatus.Done]: 2
    }[this.normalizeStatus(status)];
  }

  private priorityToNumber(priority: TaskPriority | number | string): number {
    return {
      [TaskPriority.Low]: 0,
      [TaskPriority.Medium]: 1,
      [TaskPriority.High]: 2
    }[this.normalizePriority(priority)];
  }

  private typeToNumber(type: TaskType | number | string): number {
    return {
      [TaskType.Task]: 0,
      [TaskType.Bug]: 1,
      [TaskType.Feature]: 2
    }[this.normalizeType(type)];
  }

  private normalizeQaStatus(status: QaTestCaseStatus | number | string): QaTestCaseStatus {
    if (status === 0 || status === '0') return QaTestCaseStatus.Pass;
    if (status === 1 || status === '1') return QaTestCaseStatus.Fail;
    if (status === 2 || status === '2') return QaTestCaseStatus.Blocked;
    return status as QaTestCaseStatus;
  }

  private qaStatusToNumber(status: QaTestCaseStatus | number | string): number {
    return {
      [QaTestCaseStatus.Pass]: 0,
      [QaTestCaseStatus.Fail]: 1,
      [QaTestCaseStatus.Blocked]: 2
    }[this.normalizeQaStatus(status)];
  }
}
