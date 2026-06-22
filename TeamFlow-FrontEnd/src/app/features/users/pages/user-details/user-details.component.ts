import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../services/users.service';
import { UserDto } from '../../../../core/models/user.model';

@Component({
  selector: 'app-user-details',
  templateUrl: './user-details.component.html',
  styleUrls: ['./user-details.component.css']
})
export class UserDetailsComponent implements OnInit {

  user?: UserDto;
  loading = true;
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

  goBack() {
    this.router.navigate(['/users']);
  }

  editUser() {
    if (this.user) {
      this.router.navigate(['/users/edit', this.user.id]);
    }
  }
}