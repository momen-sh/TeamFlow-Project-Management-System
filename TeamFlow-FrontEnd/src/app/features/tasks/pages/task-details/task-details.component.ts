import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TaskService } from '../../services/task.service';
import { TaskDto } from '../../../../core/models/task.model';
import { CommentService } from '../../services/comment.service';
import { CommentDto } from '../../../../core/models/comment.model';
import { NotificationService } from '../../../../core/services/notification.service';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { AppRoles } from '../../../../core/models/app-roles';
import { UserService } from '../../../users/services/users.service';
import { TaskPermissionService } from '../../services/task-permission.service';
import { UserDto } from '../../../../core/models/user.model';

@Component({
  selector: 'app-task-details',
  templateUrl: './task-details.component.html',
  styleUrls: ['./task-details.component.css']
})
export class TaskDetailsComponent implements OnInit {

  task?: TaskDto;
  comments: CommentDto[] = [];
  loading = true;
  commentsLoading = true;
  savingComment = false;
  error = '';
  commentError = '';
  commentForm: FormGroup;
  assignableUsers: UserDto[] = [];
  selectedAssigneeId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private taskService: TaskService,
    private commentService: CommentService,
    private notificationService: NotificationService,
    private fb: FormBuilder,
    private userService: UserService,
    public permissionService: TaskPermissionService,
    private authService: AuthService,
    private router: Router,
  ) {
    this.commentForm = this.fb.nonNullable.group({
      content: ['', [Validators.required, Validators.maxLength(1000)]]
    });
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.taskService.getTaskById(id).subscribe({
      next: (res: TaskDto) => {
        this.task = res;
        this.loading = false;
        this.loadUsers();
      },
      error: (err: Error) => {
        this.error = err.message || 'Failed to load task';
        this.loading = false;
      }
    });

    this.loadComments(id);
  }

  private loadUsers(): void {
    this.userService.getUsers().subscribe({
      next: users => {
        this.assignableUsers = users.filter(u => u.role === 'Developer' || u.role === 'QA');
      }
    });
  }

  onAssigneeChange(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
    this.selectedAssigneeId = val ? Number(val) : null;
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
      next: task => {
        this.task = task;
        this.notificationService.success('Task assigned.');
      },
      error: (err: Error) => this.notificationService.error(err.message || 'Assign failed.')
    });
  }

  loadComments(taskId: number): void {
    this.commentsLoading = true;
    this.commentError = '';

    this.commentService.getComments(taskId).subscribe({
      next: comments => {
        this.comments = comments;
        this.commentsLoading = false;
      },
      error: (err: Error) => {
        this.commentError = err.message || 'Failed to load comments';
        this.commentsLoading = false;
      }
    });
  }

  addComment(): void {
    if (!this.task?.id || this.commentForm.invalid) return;

    this.savingComment = true;
    this.commentError = '';

    const content = String(this.commentForm.get('content')?.value ?? '').trim();
    this.commentService.createComment(this.task.id, { content }).subscribe({
      next: comment => {
        this.comments = [...this.comments, comment];
        this.commentForm.reset();
        this.savingComment = false;
        this.notificationService.success('Comment added.');
      },
      error: (err: Error) => {
        this.commentError = err.message || 'Failed to add comment';
        this.savingComment = false;
      }
    });
  }

  unassign(): void {
    if (!this.task?.id) return;

    this.taskService.unassignTask(this.task.id).subscribe({
      next: (task: TaskDto) => {
        this.task = task;
        this.notificationService.success('Task unassigned.');
      },
      error: (err: Error) => {
        this.notificationService.error(err.message || 'Unassign failed.');
      }
    });
  }

  canUnassign(): boolean {
    if (!this.task) return false;

    const hasAssigneeId = this.task.assignedUserId !== null && this.task.assignedUserId !== undefined;
    const hasAssigneeName = !!this.task.assignedUserName;

    if (this.task.permissions?.canUnassign) return true;
    if (hasAssigneeId || hasAssigneeName) return true;

    const role = this.authService.getUserRole();
    return role === AppRoles.TeamLeader;
  }
  back(): void {
  this.router.navigate(['/tasks']);
}
}
