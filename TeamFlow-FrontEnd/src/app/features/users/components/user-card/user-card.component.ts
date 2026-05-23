import { Component, Input, Output, EventEmitter } from '@angular/core';
import { UserDto } from '../../../../core/models/user.model';

@Component({
  selector: 'app-user-card',
  templateUrl: './user-card.component.html',
  styleUrls: ['./user-card.component.css']
})
export class UserCardComponent {

  @Input() user!: UserDto;

  @Output() view = new EventEmitter<number>();
  @Output() edit = new EventEmitter<number>();
  @Output() delete = new EventEmitter<number>();

  onView() {
    this.view.emit(this.user.id);
  }

  onEdit() {
    this.edit.emit(this.user.id);
  }

  onDelete() {
    this.delete.emit(this.user.id);
  }

  get fullName(): string {
    return `${this.user.firstName} ${this.user.lastName}`;
  }
}
