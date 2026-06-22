import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ProjectListComponent } from './pages/project-list/project-list.component';
import { ProjectCreateComponent } from './pages/project-create/project-create.component';
import { ProjectDetailsComponent } from './pages/project-details/project-details.component';
import { ProjectEditComponent } from './pages/project-edit/project-edit.component';
import { RoleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  {
    path: '',
    component: ProjectListComponent
  },
  {
    path: 'create',
    canActivate: [RoleGuard],
    data: { roles: ['Admin', 'TeamLeader'] },
    component: ProjectCreateComponent
  },
  {
    path: 'details/:id',
    component: ProjectDetailsComponent
  },
  {
    path: 'edit/:id',
    canActivate: [RoleGuard],
    data: { roles: ['Admin', 'TeamLeader'] },
    component: ProjectEditComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ProjectsRoutingModule {}
