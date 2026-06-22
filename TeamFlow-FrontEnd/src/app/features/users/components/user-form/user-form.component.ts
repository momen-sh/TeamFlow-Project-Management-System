import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserDto } from '../../../../core/models/user.model';
import { APP_ROLES, AppRoles } from '../../../../core/models/app-roles';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.css']
})
export class UserFormComponent implements OnChanges {

  @Input() user?: UserDto;
  @Output() save = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();

  form: FormGroup;

  readonly roles = APP_ROLES;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      role: [AppRoles.Developer, Validators.required],
      password: ['']
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.user) {
      this.form.patchValue(this.user);

      // في edit ما نطلب password
      this.form.get('password')?.clearValidators();
      this.form.get('password')?.updateValueAndValidity();
    } else {
      // في create password إجباري
      this.form.get('password')?.setValidators([Validators.required, Validators.minLength(6)]);
      this.form.get('password')?.updateValueAndValidity();
    }
  }

  submit() {
    if (this.form.invalid) return;

    let data: any;

    if (this.user) {
      data = {
        id: this.user.id,
        firstName: this.form.value.firstName,
        lastName: this.form.value.lastName,
        email: this.form.value.email,
        role: this.form.value.role
      };
      this.save.emit(data);
      console.log('Emitting update data:', data);
    } else {
      data = {
        ...this.form.value
      };
    }

  }

  onCancel() {
    this.cancel.emit();
  }
}
