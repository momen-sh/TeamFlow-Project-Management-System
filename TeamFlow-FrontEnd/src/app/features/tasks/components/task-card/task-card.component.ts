import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { TaskDto, TaskPriority, TaskType } from '../../../../core/models/task.model';

@Component({
  selector: 'app-task-card',
  templateUrl: './task-card.component.html',
  styleUrls: ['./task-card.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TaskCardComponent {
  @Input() task!: TaskDto;
  @Input() canManage = false;
  @Input() canDelete = false;
  @Input() canSelfAssign = false;
  @Input() projectName = 'Project unavailable';
  @Input() assigneeName = '';
  @Input() canUnassign = false;
  @Input() canEdit = false;

  @Output() details = new EventEmitter<number>();
  @Output() edit = new EventEmitter<number>();
  @Output() delete = new EventEmitter<number>();
  @Output() selfAssign = new EventEmitter<void>();
  @Output() unassign = new EventEmitter<void>();

  priorityLabel(priority: TaskPriority): string {
    return this.normalizePriority(priority);
  }

  priorityClass(priority: TaskPriority): string {
    return this.normalizePriority(priority).toLowerCase();
  }

  typeLabel(type?: TaskType): string {
    return this.normalizeType(type ?? TaskType.Task);
  }

  typeIcon(type?: TaskType): string {
    return {
      Task: 'check_circle',
      Bug: 'bug_report',
      Feature: 'auto_awesome'
    }[this.normalizeType(type ?? TaskType.Task)];
  }

  typeClass(type?: TaskType): string {
    return this.normalizeType(type ?? TaskType.Task).toLowerCase();
  }

  openDetails(): void {
    this.details.emit(this.task.id);
  }

  editTask(event: MouseEvent): void {
    event.stopPropagation();
    this.edit.emit(this.task.id);
  }

  deleteTask(event: MouseEvent): void {
    event.stopPropagation();
    this.delete.emit(this.task.id);
  }

  assignToMe(event: MouseEvent): void {
    event.stopPropagation();
    this.selfAssign.emit();
  }
  unassignTask(event: MouseEvent): void {
    event.stopPropagation();
    this.unassign.emit();
  }

  private normalizePriority(priority: TaskPriority | number | string): TaskPriority {
    if (priority === 0 || priority === '0') return TaskPriority.Low;
    if (priority === 1 || priority === '1') return TaskPriority.Medium;
    if (priority === 2 || priority === '2') return TaskPriority.High;
    return priority as TaskPriority;
  }

  private normalizeType(type: TaskType | number | string): TaskType {
    if (type === 0 || type === '0') return TaskType.Task;
    if (type === 1 || type === '1') return TaskType.Bug;
    if (type === 2 || type === '2') return TaskType.Feature;
    return type as TaskType;
  }
}
