import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { UserListComponent } from './pages/user-list/user-list.component';
import { UserCreateComponent } from './pages/user-create/user-create.component';
import { UserDetailsComponent } from './pages/user-details/user-details.component';
import { UserEditComponent } from './pages/user-edit/user-edit.component';
import { RoleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  {
    path: '',
    canActivate: [RoleGuard],
    data: { roles: ['Admin'] },
    component: UserListComponent
  },
  {
    path: 'create',
    canActivate: [RoleGuard],
    data: { roles: ['Admin'] },
    component: UserCreateComponent
  },
  {
    path: 'details/:id',
    canActivate: [RoleGuard],
    data: { roles: ['Admin'] },
    component: UserDetailsComponent
  },
  {
    path: 'edit/:id',
    canActivate: [RoleGuard],
    data: { roles: ['Admin'] },
    component: UserEditComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersRoutingModule {}
