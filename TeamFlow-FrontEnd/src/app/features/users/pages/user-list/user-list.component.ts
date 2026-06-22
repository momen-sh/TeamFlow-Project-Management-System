import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../services/users.service';
import { UserDto } from '../../../../core/models/user.model';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {

  users: UserDto[] = [];
  loading = true;
  error = '';

  constructor(
    private userService: UserService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers() {
    this.loading = true;

    this.userService.getUsers().subscribe({
      next: (res) => {
        this.users = res;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load users';
        this.loading = false;
      }
    });
  }

  openUser(id: number) {
    this.router.navigate(['/users/details', id]);
  }

  editUser(id: number) {
    this.router.navigate(['/users/edit', id]);
  }

  deleteUser(id: number) {
    if (!confirm('Are you sure you want to delete this user?')) return;

    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.users = this.users.filter(u => u.id !== id);
        this.notificationService.success('User deleted successfully.');
      },
      error: () => {
        this.error = 'Failed to delete user';
      }
    });
  }

  createUser() {
    this.router.navigate(['/users/create']);
  }
}
