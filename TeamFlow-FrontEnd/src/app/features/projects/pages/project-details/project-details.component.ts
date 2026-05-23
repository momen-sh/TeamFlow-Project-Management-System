import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, combineLatest, map, startWith } from 'rxjs';
import { ProjectService } from '../../services/project.service';
import { ProjectDto } from '../../../../core/models/project.model';
import { AuthService } from '../../../../core/services/auth.service';
import { UserDto } from '../../../../core/models/user.model';
import { UserService } from '../../../users/services/users.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AppRoles } from '../../../../core/models/app-roles';

type AssignMemberForm = FormGroup<{
  userId: FormControl<number | null>;
  role: FormControl<string>;
}>;

@Component({
  selector: 'app-project-details',
  templateUrl: './project-details.component.html',
  styleUrls: ['./project-details.component.css']
})
export class ProjectDetailsComponent implements OnInit {

  project?: ProjectDto;
  users: UserDto[] = [];
  loading = true;
  assigning = false;
  error = '';
  memberSearch = this.fb.nonNullable.control('');
  assignForm: AssignMemberForm;

  private readonly usersSubject = new BehaviorSubject<UserDto[]>([]);
  readonly filteredUsers$ = combineLatest([
    this.usersSubject.asObservable(),
    this.memberSearch.valueChanges.pipe(startWith(''))
  ]).pipe(
    map(([users, search]) => this.filterAssignableUsers(users, search))
  );

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    private authService: AuthService,
    private userService: UserService,
    private notificationService: NotificationService,
    private fb: FormBuilder
  ) {
    this.assignForm = this.fb.group({
      userId: this.fb.control<number | null>(null, Validators.required),
      role: this.fb.nonNullable.control('Member', [Validators.required, Validators.maxLength(50)])
    });
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.projectService.getProjectById(id).subscribe({
      next: (res: ProjectDto) => {
        this.project = res;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load project';
        this.loading = false;
      }
    });

    this.userService.getUsers().subscribe({
      next: users => {
        this.users = users;
        this.usersSubject.next(users);
      }
    });
  }

  goBack() {
    this.router.navigate(['/projects']);
  }

  editProject() {
    if (!this.project?.id) return;
    this.router.navigate(['/projects/edit', this.project.id]);
  }

  canManageProjects(): boolean {
    const role = this.authService.getUserRole();
    return role === AppRoles.Admin || role === AppRoles.TeamLeader;
  }

  selectUser(user: UserDto, event?: { isUserInput?: boolean }): void {
    if (event && !event.isUserInput) return;

    this.assignForm.controls.userId.setValue(user.id);
    this.memberSearch.setValue(this.userLabel(user), { emitEvent: false });
  }

  assignMember(): void {
    if (!this.project || this.assignForm.invalid) {
      this.assignForm.markAllAsTouched();
      return;
    }

    const value = this.assignForm.getRawValue();
    if (value.userId === null) return;

    this.assigning = true;
    this.projectService.assignMembers(this.project.id, [value.userId]).subscribe({
      next: () => {
        this.notificationService.success('User assigned to project.');
        this.assigning = false;
        this.memberSearch.setValue('');
        this.assignForm.reset({ userId: null, role: 'Member' });
        this.reloadProject();
      },
      error: (err: Error) => {
        this.notificationService.error(err.message || 'Could not assign user.');
        this.assigning = false;
      }
    });
  }

  userLabel(user: UserDto): string {
    return `${user.firstName} ${user.lastName}`.trim()
      ? `${user.firstName} ${user.lastName} · ${user.email}`
      : user.email;
  }

  memberName(userId: number): string {
    const user = this.users.find(item => item.id === userId);
    return user ? this.userLabel(user) : `User #${userId}`;
  }

  private reloadProject(): void {
    if (!this.project) return;
    this.projectService.getProjectById(this.project.id).subscribe({
      next: project => {
        this.project = project;
      }
    });
  }

  confirmRemoveMember(userId: number): void {
    if (!this.project) return;
    if (!this.canManageProjects()) return;
    const confirmed = confirm('Remove this user from the project? They will lose access to all project tasks. Continue?');
    if (!confirmed) return;
    this.removeMember(userId);
  }

  removeMember(userId: number): void {
    if (!this.project) return;
    this.projectService.removeMember(this.project.id, userId).subscribe({
      next: () => {
        this.notificationService.success('Project member removed.');
        this.reloadProject();
      },
      error: (err: any) => {
        this.notificationService.error(err?.message || 'Failed to remove project member.');
      }
    });
  }

  private filterAssignableUsers(users: UserDto[], search: string): UserDto[] {
    const query = search.trim().toLowerCase();
    const assignedIds = new Set([
      this.project?.ownerId,
      ...(this.project?.members ?? []).map(member => member.userId)
    ].filter((id): id is number => typeof id === 'number'));

    return users.filter(user => {
      const matchesSearch = !query || this.userLabel(user).toLowerCase().includes(query);
      return matchesSearch && !assignedIds.has(user.id);
    });
  }
} 
