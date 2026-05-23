import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { ProjectsRoutingModule } from './projects-routing.module';
import { SharedModule } from '../../shared/shared.module';

// Pages
import { ProjectListComponent } from './pages/project-list/project-list.component';
import { ProjectCreateComponent } from './pages/project-create/project-create.component';
import { ProjectDetailsComponent } from './pages/project-details/project-details.component';
import { ProjectEditComponent } from './pages/project-edit/project-edit.component';

// Components
import { ProjectCardComponent } from './components/project-card/project-card.component';
import { ProjectFormComponent } from './components/project-form/project-form.component';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@NgModule({
  declarations: [
    ProjectListComponent,
    ProjectCreateComponent,
    ProjectDetailsComponent,
    ProjectEditComponent,
    ProjectCardComponent,
    ProjectFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    SharedModule,
    ProjectsRoutingModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatIconModule,
    MatButtonModule
  ]
})
export class ProjectsModule {}
