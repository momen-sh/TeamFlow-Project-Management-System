import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { KanbanBoardComponent } from './components/kanban-board/kanban-board.component';
import { TaskCreateComponent } from './components/task-create/task-create.component';
import { TaskDetailsComponent } from './components/task-details/task-details.component';
import { TaskEditComponent } from './components/task-edit/task-edit.component';
import { AuthGuard } from '../../core/guards/auth.guard';

const routes: Routes = [
  { path: '', component: KanbanBoardComponent, canActivate: [AuthGuard] },
  { path: 'create', component: TaskCreateComponent, canActivate: [AuthGuard] },
  { path: ':id/edit', component: TaskEditComponent, canActivate: [AuthGuard] },
  { path: ':id', component: TaskDetailsComponent, canActivate: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TasksRoutingModule {}
