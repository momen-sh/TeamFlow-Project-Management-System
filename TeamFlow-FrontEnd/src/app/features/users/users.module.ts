import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { UsersRoutingModule } from './users-routing.module';
import { UserListComponent } from './pages/user-list/user-list.component';
import { UserCreateComponent } from './pages/user-create/user-create.component';
import { UserEditComponent } from './pages/user-edit/user-edit.component';
import { UserDetailsComponent } from './pages/user-details/user-details.component';
import { UserFormComponent } from './components/user-form/user-form.component';
import { UserCardComponent } from './components/user-card/user-card.component';

import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [
    UserListComponent,
    UserCreateComponent,
    UserEditComponent,
    UserDetailsComponent,
    UserFormComponent,
    UserCardComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    UsersRoutingModule,
    SharedModule
  ]
})
export class UsersModule {}
