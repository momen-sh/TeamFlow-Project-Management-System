import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ProjectService } from '../../../projects/services/project.service';
import { UserDto } from '../../../../core/models/user.model';
import { UserService } from '../../../users/services/users.service';
import { CommentDto } from '../../../../core/models/comment.model';
import {
  QaTestCaseDto,
  QaTestCaseStatus,
  TaskAttachmentDto,
  TaskDto,
  TaskPriority,
  TaskStatus,
  TaskType,
  TaskWorkRecordDto
} from '../../../../core/models/task.model';
import { TaskAttachmentService } from '../../services/task-attachment.service';
import { TaskPermissionService } from '../../services/task-permission.service';
import { TaskService } from '../../services/task.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CommentService } from '../../services/comment.service';

@Component({
  selector: 'app-task-details',
  templateUrl: './task-details.component.html',
  styleUrls: ['./task-details.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskDetailsComponent implements OnInit {
  task?: TaskDto;
  loading = true;
  error = '';
  attachmentError = '';
  fragment: string | null = null;
  projectName = 'Project unavailable';
  assigneeName = '';
  assignableUsers: UserDto[] = [];
  selectedAssigneeId: number | null = null;
  attachments: TaskAttachmentDto[] = [];
  workRecords: TaskWorkRecordDto[] = [];
  qaTestCases: QaTestCaseDto[] = [];
  comments: CommentDto[] = [];
  users: UserDto[] = [];
  qaUsers: UserDto[] = [];
  selectedQaUserIds: number[] = [];
  mentionedUserIds: number[] = [];
  qaModalOpen = false;
  savingWorkRecord = false;
  savingQaTestCase = false;
  savingComment = false;
  sendingToQa = false;
  readonly qaStatuses = Object.values(QaTestCaseStatus);
  workRecordForm: FormGroup;
  qaTestCaseForm: FormGroup;
  commentForm: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService,
    private attachmentService: TaskAttachmentService,
    public permissionService: TaskPermissionService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef,
    private projectService: ProjectService,
    private userService: UserService,
    private commentService: CommentService,
    private fb: FormBuilder
  ) {
    this.workRecordForm = this.fb.nonNullable.group({
      title: ['', [Validators.required, Validators.maxLength(150)]],
      timeSpentHours: [0, [Validators.min(0)]],
      timeSpentMinutes: [30, [Validators.min(0), Validators.max(59)]],
      branchNumber: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(1000)]]
    });

    this.qaTestCaseForm = this.fb.nonNullable.group({
      title: ['', [Validators.required, Validators.maxLength(150)]],
      steps: ['', [Validators.required, Validators.maxLength(2000)]],
      expectedResult: ['', [Validators.required, Validators.maxLength(1000)]],
      actualResult: ['', [Validators.maxLength(1000)]],
      status: [QaTestCaseStatus.Pass, [Validators.required]]
    });

    this.commentForm = this.fb.nonNullable.group({
      content: ['', [Validators.required, Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.fragment = this.route.snapshot.fragment;

    this.taskService.getTaskById(id).subscribe({
      next: task => {
        console.log(task);
        console.log(task.permissions);
        this.task = task;
        this.attachments = task.attachments ?? [];
        this.loading = false;
        this.loadNames(task);
        this.loadAttachments(task.id);
        this.loadWorkRecords(task.id);
        this.loadQaTestCases(task.id);
        this.loadComments(task.id);
        this.loadUsers();
        this.cdr.markForCheck();
        this.scrollToFragment();
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load task';
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  statusLabel(status: TaskStatus): string {
    const normalized = this.normalizeStatus(status);
    return normalized === TaskStatus.ToDo ? 'To Do' : normalized === TaskStatus.InProgress ? 'In Progress' : 'Done';
  }

  priorityLabel(priority: TaskPriority): string {
    return this.normalizePriority(priority);
  }

  typeLabel(type?: TaskType): string {
    return this.normalizeType(type ?? TaskType.Task);
  }

  typeIcon(type?: TaskType): string {
    return {
      Task: 'check_circle',
      Bug: 'bug_report',
      Feature: 'auto_awesome'
    }[this.normalizeType(type ?? TaskType.Task)];
  }

  editTask(): void {
    if (!this.task) return;
    this.router.navigate(['/tasks', this.task.id, 'edit']);
  }

  back(): void {
    this.router.navigate(['/tasks']);
  }

  canManageTasks(): boolean {
    return !!this.task && this.permissionService.canModifyTask(this.task);
  }

  canSelfAssign(): boolean {
    return !!this.task && this.permissionService.canSelfAssign(this.task);
  }
  

  selfAssign(): void {
    if (!this.task || !this.canSelfAssign()) return;

    this.taskService.selfAssignTask(this.task.id).subscribe({
      next: task => {
        this.task = task;
        this.notificationService.success('Task assigned to you.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => this.notificationService.error(err.message || 'Self assignment failed.')
    });
  }

  unassign(): void {
    if (!this.task || !this.task.id) return;

    this.taskService.unassignTask(this.task.id).subscribe({
      next: task => {
        this.task = task;
        this.notificationService.success('Task unassigned.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => this.notificationService.error(err.message || 'Unassign failed.')
    });
  }

  openQaModal(): void {
    if (!this.task || !this.task.permissions?.canSendToQa) return;

    this.selectedQaUserIds = this.task.qaAssignments?.map(assignment => assignment.qaUserId) ?? [];
    this.qaModalOpen = true;
    this.loadQaUsers();
    this.cdr.markForCheck();
  }

  closeQaModal(): void {
    this.qaModalOpen = false;
    this.cdr.markForCheck();
  }

  toggleQaUser(userId: number): void {
    this.selectedQaUserIds = this.selectedQaUserIds.includes(userId)
      ? this.selectedQaUserIds.filter(id => id !== userId)
      : [...this.selectedQaUserIds, userId];
    this.cdr.markForCheck();
  }

  sendToQa(): void {
    if (!this.task || !this.task.permissions?.canSendToQa || this.selectedQaUserIds.length === 0) return;

    this.sendingToQa = true;
    this.taskService.sendToQa(this.task.id, { qaUserIds: this.selectedQaUserIds }).subscribe({
      next: task => {
        this.task = task;
        this.sendingToQa = false;
        this.qaModalOpen = false;
        this.notificationService.success('Task sent to QA.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.sendingToQa = false;
        this.notificationService.error(err.message || 'Send to QA failed.');
        this.cdr.markForCheck();
      }
    });
  }

  updateQaStatus(testCase: QaTestCaseDto, status: QaTestCaseStatus): void {
    if (!this.task || testCase.status === status) return;

    this.taskService.updateQaTestCaseStatus(this.task.id, testCase.id, status).subscribe({
      next: updated => {
        this.qaTestCases = this.qaTestCases.map(item => item.id === updated.id ? updated : item);
        this.notificationService.success('QA status updated.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => this.notificationService.error(err.message || 'QA status update failed.')
    });
  }

  addWorkRecord(): void {
    if (!this.task || this.workRecordForm.invalid) return;

    const hours = Number(this.workRecordForm.value.timeSpentHours ?? 0);
    const minutes = Number(this.workRecordForm.value.timeSpentMinutes ?? 0);
    const timeSpentMinutes = hours * 60 + minutes;
    if (timeSpentMinutes <= 0) {
      this.notificationService.error('Time spent must be greater than zero.');
      return;
    }

    this.savingWorkRecord = true;
    this.taskService.createWorkRecord(this.task.id, {
      title: String(this.workRecordForm.value.title ?? '').trim(),
      description: String(this.workRecordForm.value.description ?? '').trim() || null,
      branchNumber: String(this.workRecordForm.value.branchNumber ?? '').trim(),
      timeSpentMinutes
    }).subscribe({
      next: record => {
        this.workRecords = [record, ...this.workRecords];
        this.workRecordForm.reset({ title: '', timeSpentHours: 0, timeSpentMinutes: 30, branchNumber: '', description: '' });
        this.savingWorkRecord = false;
        this.notificationService.success('Work record added.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.savingWorkRecord = false;
        this.notificationService.error(err.message || 'Work record could not be added.');
        this.cdr.markForCheck();
      }
    });
  }

  addQaTestCase(): void {
    if (!this.task || this.qaTestCaseForm.invalid) return;

    this.savingQaTestCase = true;
    this.taskService.createQaTestCase(this.task.id, {
      title: String(this.qaTestCaseForm.value.title ?? '').trim(),
      steps: String(this.qaTestCaseForm.value.steps ?? '').trim(),
      expectedResult: String(this.qaTestCaseForm.value.expectedResult ?? '').trim(),
      actualResult: String(this.qaTestCaseForm.value.actualResult ?? '').trim() || null,
      status: this.qaTestCaseForm.value.status
    }).subscribe({
      next: testCase => {
        this.qaTestCases = [testCase, ...this.qaTestCases];
        this.qaTestCaseForm.reset({ title: '', steps: '', expectedResult: '', actualResult: '', status: QaTestCaseStatus.Pass });
        this.savingQaTestCase = false;
        this.notificationService.success('QA test case added.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.savingQaTestCase = false;
        this.notificationService.error(err.message || 'QA test case could not be added.');
        this.cdr.markForCheck();
      }
    });
  }

  addComment(): void {
    if (!this.task || this.commentForm.invalid) return;

    this.savingComment = true;
    this.commentService.createComment(this.task.id, {
      content: String(this.commentForm.value.content ?? '').trim(),
      mentionedUserIds: this.mentionedUserIds
    }).subscribe({
      next: comment => {
        this.comments = [...this.comments, comment];
        this.commentForm.reset({ content: '' });
        this.mentionedUserIds = [];
        this.savingComment = false;
        this.notificationService.success('Comment added.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        this.savingComment = false;
        this.notificationService.error(err.message || 'Comment could not be added.');
        this.cdr.markForCheck();
      }
    });
  }

  mentionSuggestions(): UserDto[] {
    const content = String(this.commentForm.value.content ?? '');
    const match = content.match(/@([\w.@-]*)$/);
    if (!match) return [];
    const query = match[1].toLowerCase();
    return this.users
      .filter(user => !this.mentionedUserIds.includes(user.id))
      .filter(user => this.fullName(user).toLowerCase().includes(query) || user.email.toLowerCase().includes(query))
      .slice(0, 5);
  }

  addMention(user: UserDto): void {
    const content = String(this.commentForm.value.content ?? '');
    const nextContent = content.replace(/@([\w.@-]*)$/, `@${this.fullName(user)} `);
    this.commentForm.patchValue({ content: nextContent });
    this.mentionedUserIds = [...this.mentionedUserIds, user.id];
    this.cdr.markForCheck();
  }

  mentionedUsers(comment: CommentDto): UserDto[] {
    const ids = comment.mentionedUserIds ?? [];
    return this.users.filter(user => ids.includes(user.id));
  }

  highlightComment(comment: CommentDto): string {
    const text = this.escapeHtml(comment.content);
    const users = this.mentionedUsers(comment);
    if (!users.length) return text;

    return users.reduce((html, user) => {
      const display = this.escapeHtml(`@${this.fullName(user)}`);
      const email = this.escapeHtml(`@${user.email}`);
      const pattern = new RegExp(`(${this.escapeRegExp(display)}|${this.escapeRegExp(email)})`, 'gi');
      return html.replace(pattern, '<span class="mention-highlight">$1</span>');
    }, text);
  }

  userNameById(id: number): string {
    const user = this.users.find(item => item.id === id);
    return user ? this.fullName(user) : `User #${id}`;
  }

  formatDuration(totalMinutes: number): string {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    if (hours && minutes) return `${hours}h ${minutes}m`;
    if (hours) return `${hours}h`;
    return `${minutes}m`;
  }

  statusClass(status: QaTestCaseStatus): string {
    return String(status).toLowerCase();
  }

  isImage(attachment: TaskAttachmentDto): boolean {
    return this.contentType(attachment).startsWith('image/');
  }

  isVideo(attachment: TaskAttachmentDto): boolean {
    return this.contentType(attachment).startsWith('video/');
  }

  attachmentUrl(attachment: TaskAttachmentDto): string {
    return attachment.url || attachment.fileUrl || '';
  }

  attachmentSize(attachment: TaskAttachmentDto): number {
    return attachment.size ?? attachment.fileSize ?? 0;
  }

  fileSize(size: number | undefined): string {
    size = size ?? 0;
    if (size < 1024 * 1024) return `${Math.ceil(size / 1024)} KB`;
    return `${(size / (1024 * 1024)).toFixed(1)} MB`;
  }

  private loadNames(task: TaskDto): void {
    if (task.projectName) {
      this.projectName = task.projectName;
    } else {
      this.projectService.getProjectById(task.projectId).subscribe({
        next: project => {
          this.projectName = project.name;
          this.cdr.markForCheck();
        }
      });
    }

    if (task.assignedUserName) {
      this.assigneeName = task.assignedUserName;
    } else if (task.assignedUserId && this.authService.isAdmin()) {
      this.userService.getUserById(task.assignedUserId).subscribe({
        next: user => {
          this.assigneeName = this.fullName(user);
          this.cdr.markForCheck();
        }
      });
    }
  }

  private loadAttachments(taskId: number): void {
    this.attachmentService.getAttachments(taskId).subscribe({
      next: attachments => {
        this.attachments = attachments;
        this.cdr.markForCheck();
      },
      error: (err: Error) => {
        if (this.attachments.length > 0) return;
        this.attachmentError = err.message || 'Attachments are unavailable.';
        this.cdr.markForCheck();
      }
    });
  }

  private loadWorkRecords(taskId: number): void {
    this.taskService.getWorkRecords(taskId).subscribe({
      next: records => {
        this.workRecords = records;
        this.cdr.markForCheck();
      }
    });
  }

  private loadQaTestCases(taskId: number): void {
    this.taskService.getQaTestCases(taskId).subscribe({
      next: testCases => {
        this.qaTestCases = testCases;
        this.cdr.markForCheck();
      }
    });
  }

  private loadComments(taskId: number): void {
    this.commentService.getComments(taskId).subscribe({
      next: comments => {
        this.comments = comments;
        this.cdr.markForCheck();
      }
    });
  }

  private loadUsers(): void {
    this.userService.getMentionTargets().subscribe({
      next: users => {
        this.users = users;
        this.cdr.markForCheck();
      }
    });

    // Also load full user list so TeamLeaders can assign developers and QAs
    this.userService.getUsers().subscribe({
      next: users => {
        this.assignableUsers = users.filter(u => u.role === 'Developer' || u.role === 'QA');
        this.cdr.markForCheck();
      }
    });
  }

  onAssigneeChange(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
    this.selectedAssigneeId = val ? Number(val) : null;
    this.cdr.markForCheck();
  }

  assignToSelectedUser(): void {
    if (!this.task || this.selectedAssigneeId === null) return;

    const payload = {
      title: this.task.title,
      description: this.task.description ?? undefined,
      status: this.task.status,
      priority: this.task.priority,
      type: this.task.type ?? undefined,
      projectId: this.task.projectId,
      assignedUserId: this.selectedAssigneeId
    } as any;

    this.taskService.updateTask(this.task.id, payload).subscribe({
      next: updated => {
        this.task = updated;
        this.notificationService.success('Task assigned.');
        this.cdr.markForCheck();
      },
      error: (err: Error) => this.notificationService.error(err.message || 'Assign failed.')
    });
  }

  private loadQaUsers(): void {
    this.userService.getQaUsers().subscribe({
      next: users => {
        this.qaUsers = users;
        this.cdr.markForCheck();
      }
    });
  }

  private scrollToFragment(): void {
    if (this.fragment !== 'qa-test-cases') return;

    setTimeout(() => {
      document.getElementById('qa-test-cases')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }, 250);
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  private escapeRegExp(value: string): string {
    return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  fullName(user: UserDto): string {
    return `${user.firstName} ${user.lastName}`.trim() || user.email;
  }

  private contentType(attachment: TaskAttachmentDto): string {
    return attachment.contentType || attachment.fileType || '';
  }

  canEditTask(): boolean {
    return !!this.task?.permissions?.canManage;
  }

  canUnassign(): boolean {
    // Show Unassign when the task appears assigned — either an assignee id or name is present
    if (!this.task) return false;
    const hasAssigneeId = this.task.assignedUserId !== null && this.task.assignedUserId !== undefined;
    const hasAssigneeName = !!this.task.assignedUserName || !!this.assigneeName;
    return hasAssigneeId || hasAssigneeName;
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
