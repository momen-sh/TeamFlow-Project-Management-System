import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ButtonComponent } from './components/button/button.component';
import { InputComponent } from './components/input/input.component';
import { ModalComponent } from './components/modal/modal.component';
import { TruncatePipe } from './pipes/truncate.pipe';
import { MaterialModule } from './material.module';

@NgModule({
  declarations: [
    ButtonComponent,
    InputComponent,
    ModalComponent,
    TruncatePipe
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule
  ],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    ButtonComponent,
    InputComponent,
    ModalComponent,
    TruncatePipe
  ]
})
export class SharedModule { }
