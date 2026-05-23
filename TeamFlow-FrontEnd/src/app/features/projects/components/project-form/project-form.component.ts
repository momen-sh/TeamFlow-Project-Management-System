import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { CreateProjectDto, ProjectDto } from '../../../../core/models/project.model';

type ProjectFormGroup = FormGroup<{
  name: FormControl<string>;
  description: FormControl<string>;
}>;

@Component({
  selector: 'app-project-form',
  templateUrl: './project-form.component.html',
  styleUrls: ['./project-form.component.css']
})
export class ProjectFormComponent implements OnChanges {

  @Input() project?: ProjectDto;
  @Input() saving = false;
  @Output() save = new EventEmitter<CreateProjectDto>();
  @Output() cancel = new EventEmitter<void>();

  form: ProjectFormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.nonNullable.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['']
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.project) {
      this.form.patchValue(this.project);
    }
  }

  submit() {
    if (this.form.invalid) return;

    const value = this.form.getRawValue();
    const data: CreateProjectDto = {
      name: value.name.trim(),
      description: value.description.trim() || undefined,
      workspaceId: this.project?.workspaceId
    };

    this.save.emit(data);
  }

  onCancel() {
    this.cancel.emit();
  }
}
