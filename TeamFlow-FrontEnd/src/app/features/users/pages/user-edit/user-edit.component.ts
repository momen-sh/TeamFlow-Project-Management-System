import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/users.service';
import { UserDto } from '../../../../core/models/user.model';

@Component({
  selector: 'app-user-edit',
  templateUrl: './user-edit.component.html',
  styleUrls: ['./user-edit.component.css']
})
export class UserEditComponent implements OnInit {

  user?: UserDto;
  loading = true;
  saving = false;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadUser(id);
  }

  loadUser(id: number) {
    this.loading = true;

    this.userService.getUserById(id).subscribe({
      next: (res) => {
        this.user = res;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load user';
        this.loading = false;
      }
    });
  }

  updateUser(data: any) {

    if (!this.user) return;

    this.saving = true;
    this.error = '';

    this.userService.updateUser(this.user.id!, data).subscribe({
      next: () => {
        this.router.navigate(['/users/details', this.user!.id]);
      },
      error: () => {
        this.error = 'Failed to update user';
        this.saving = false;
      }
    });
  }

  cancel() {
    this.router.navigate(['/users']);
  }
}
