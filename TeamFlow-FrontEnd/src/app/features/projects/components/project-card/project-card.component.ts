import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ProjectDto } from '../../../../core/models/project.model';

@Component({
  selector: 'app-project-card',
  templateUrl: './project-card.component.html',
  styleUrls: ['./project-card.component.css']
})
export class ProjectCardComponent {

  @Input() project!: ProjectDto;
  @Input() canManage = false;

  @Output() view = new EventEmitter<number>();
  @Output() edit = new EventEmitter<number>();
  @Output() delete = new EventEmitter<number>();

  onView() {
    this.view.emit(this.project.id);
  }

  onEdit() {
    if (!this.canManage) return;
    this.edit.emit(this.project.id);
  }

  onDelete() {
    if (!this.canManage) return;
    this.delete.emit(this.project.id);
  }
}
