import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, combineLatest, map, startWith } from 'rxjs';
import { ProjectDto } from '../../../../core/models/project.model';
import { ProjectService } from '../../../projects/services/project.service';
import { UserDto } from '../../../../core/models/user.model';
import { UserService } from '../../../users/services/users.service';
import { CreateTaskDto, TaskDto, TaskPriority, TaskStatus, TaskType } from '../../../../core/models/task.model';

export interface TaskFormSubmit {
  task: CreateTaskDto;
  files: File[];
}

type TaskFormGroup = FormGroup<{
  title: FormControl<string>;
  description: FormControl<string>;
  status: FormControl<TaskStatus>;
  priority: FormControl<TaskPriority>;
  type: FormControl<TaskType>;
  projectId: FormControl<number | null>;
  assignedUserId: FormControl<number | null>;
}>;

@Component({
  selector: 'app-task-form',
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskFormComponent implements OnChanges, OnInit {
  @Input() task?: TaskDto;
  @Input() saving = false;
  @Output() save = new EventEmitter<TaskFormSubmit>();
  @Output() cancel = new EventEmitter<void>();

  readonly statuses = [
    { label: 'To Do', value: TaskStatus.ToDo },
    { label: 'In Progress', value: TaskStatus.InProgress },
    { label: 'Done', value: TaskStatus.Done }
  ];
  readonly priorities = [
    { label: 'Low', value: TaskPriority.Low },
    { label: 'Medium', value: TaskPriority.Medium },
    { label: 'High', value: TaskPriority.High }
  ];
  readonly types = [
    { label: 'Task', value: TaskType.Task, icon: 'check_circle' },
    { label: 'Bug', value: TaskType.Bug, icon: 'bug_report' },
    { label: 'Feature', value: TaskType.Feature, icon: 'auto_awesome' }
  ];

  form: TaskFormGroup;
  projectSearch = this.fb.nonNullable.control('');
  userSearch = this.fb.nonNullable.control('');
  selectedFiles: File[] = [];
  attachmentPreviews: Array<{ file: File; url: string; kind: 'image' | 'video' | 'file' }> = [];

  private readonly projectsSubject = new BehaviorSubject<ProjectDto[]>([]);
  private readonly usersSubject = new BehaviorSubject<UserDto[]>([]);

  readonly filteredProjects$ = combineLatest([
    this.projectsSubject.asObservable(),
    this.projectSearch.valueChanges.pipe(startWith(''))
  ]).pipe(
    map(([projects, search]) => this.filterProjects(projects, search))
  );

  readonly filteredUsers$ = combineLatest([
    this.usersSubject.asObservable(),
    this.userSearch.valueChanges.pipe(startWith(''))
  ]).pipe(
    map(([users, search]) => this.filterUsers(users, search))
  );

  constructor(
    private fb: FormBuilder,
    private projectService: ProjectService,
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      title: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(150)]),
      description: this.fb.nonNullable.control('', [Validators.maxLength(1000)]),
      status: this.fb.nonNullable.control(TaskStatus.ToDo, Validators.required),
      priority: this.fb.nonNullable.control(TaskPriority.Medium, Validators.required),
      type: this.fb.nonNullable.control(TaskType.Task, Validators.required),
      projectId: this.fb.control<number | null>(null, Validators.required),
      assignedUserId: this.fb.control<number | null>(null)
    });
  }

  ngOnInit(): void {
    this.projectService.getProjects().subscribe({
      next: projects => {
        this.projectsSubject.next(projects);
        this.syncSelectionLabels();
        this.cdr.markForCheck();
      }
    });

    this.userService.getUsers().subscribe({
      next: users => {
        this.usersSubject.next(users);
        this.syncSelectionLabels();
        this.cdr.markForCheck();
      }
    });
  }

  ngOnChanges(): void {
    if (!this.task) return;

    this.form.patchValue({
      title: this.task.title,
      description: this.task.description ?? '',
      status: this.normalizeStatus(this.task.status),
      priority: this.normalizePriority(this.task.priority),
      type: this.normalizeType(this.task.type ?? TaskType.Task),
      projectId: this.task.projectId,
      assignedUserId: this.task.assignedUserId ?? null
    });
    this.syncSelectionLabels();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    if (value.projectId === null) return;

    this.save.emit({
      task: {
        title: value.title.trim(),
        description: value.description.trim() || undefined,
        status: value.status,
        priority: value.priority,
        type: value.type,
        projectId: Number(value.projectId),
        assignedUserId: value.assignedUserId === null ? undefined : Number(value.assignedUserId)
      },
      files: this.selectedFiles
    });
  }

  onCancel(): void {
    this.cancel.emit();
  }

  selectProject(project: ProjectDto, event?: { isUserInput?: boolean }): void {
    if (event && !event.isUserInput) return;

    this.form.controls.projectId.setValue(project.id);
    this.projectSearch.setValue(project.name, { emitEvent: false });
    const assignedUserId = this.form.controls.assignedUserId.value;
    if (assignedUserId && !this.isProjectAssignableUser(project, assignedUserId)) {
      this.clearUser();
      return;
    }

    this.userSearch.setValue(this.userSearch.value);
  }

  selectUser(user: UserDto, event?: { isUserInput?: boolean }): void {
    if (event && !event.isUserInput) return;

    this.form.controls.assignedUserId.setValue(user.id);
    this.userSearch.setValue(this.fullName(user), { emitEvent: false });
  }

  clearUser(): void {
    this.form.controls.assignedUserId.setValue(null);
    this.userSearch.setValue('');
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.setSelectedFiles(Array.from(input.files ?? []));
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  onDropFiles(event: DragEvent): void {
    event.preventDefault();
    this.setSelectedFiles(Array.from(event.dataTransfer?.files ?? []));
  }

  private setSelectedFiles(files: File[]): void {
    this.revokePreviews();
    this.selectedFiles = files;
    this.attachmentPreviews = files.map(file => ({
      file,
      url: URL.createObjectURL(file),
      kind: this.previewKind(file)
    }));
  }

  removeFile(file: File): void {
    const preview = this.attachmentPreviews.find(item => item.file === file);
    if (preview) URL.revokeObjectURL(preview.url);
    this.selectedFiles = this.selectedFiles.filter(item => item !== file);
    this.attachmentPreviews = this.attachmentPreviews.filter(item => item.file !== file);
  }

  fileSize(size: number): string {
    if (size < 1024 * 1024) return `${Math.ceil(size / 1024)} KB`;
    return `${(size / (1024 * 1024)).toFixed(1)} MB`;
  }

  fullName(user: UserDto): string {
    return `${user.firstName} ${user.lastName}`.trim() || user.email;
  }

  private syncSelectionLabels(): void {
    const projectId = this.form.controls.projectId.value;
    const assignedUserId = this.form.controls.assignedUserId.value;
    const project = this.projectsSubject.value.find(item => item.id === projectId);
    const user = this.usersSubject.value.find(item => item.id === assignedUserId);

    if (project) {
      this.projectSearch.setValue(project.name, { emitEvent: false });
    }

    if (user) {
      this.userSearch.setValue(this.fullName(user), { emitEvent: false });
    }
  }

  private filterProjects(projects: ProjectDto[], search: string): ProjectDto[] {
    const query = search.trim().toLowerCase();
    return projects.filter(project => project.name.toLowerCase().includes(query));
  }

  private filterUsers(users: UserDto[], search: string): UserDto[] {
    const query = search.trim().toLowerCase();
    const project = this.selectedProject();
    return users.filter(user => {
      const matchesSearch = `${this.fullName(user)} ${user.email}`.toLowerCase().includes(query);
      const matchesProject = !project || this.isProjectAssignableUser(project, user.id);
      return matchesSearch && matchesProject;
    });
  }

  private selectedProject(): ProjectDto | undefined {
    const projectId = this.form.controls.projectId.value;
    return this.projectsSubject.value.find(project => project.id === projectId);
  }

  private isProjectAssignableUser(project: ProjectDto, userId: number): boolean {
    if (Number(project.ownerId) === userId) return true;
    if (project.memberIds?.some(id => Number(id) === userId)) return true;
    return !!project.members?.some(member => Number(member.userId) === userId);
  }

  private previewKind(file: File): 'image' | 'video' | 'file' {
    if (file.type.startsWith('image/')) return 'image';
    if (file.type.startsWith('video/')) return 'video';
    return 'file';
  }

  private revokePreviews(): void {
    this.attachmentPreviews.forEach(preview => URL.revokeObjectURL(preview.url));
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
}
