import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserService } from '../../services/users.service';
import { CreateUserDto } from '../../../../core/models/create-user.model';

@Component({
    selector: 'app-user-create',
    templateUrl: './user-create.component.html',
    styleUrls: ['./user-create.component.css']
})
export class UserCreateComponent {

    loading = false;
    error = '';

    constructor(
        private userService: UserService,
        private router: Router
    ) { }

    createUser(user: CreateUserDto) {

        this.loading = true;
        this.error = '';

        this.userService.createUser(user).subscribe({
            next: () => {
                this.router.navigate(['/users']);
            },
            error: (err) => {
                this.error = err?.error?.message || 'Failed to create user';
                this.loading = false;
            }
        });
    }

    cancel() {
        this.router.navigate(['/users']);
    }
}