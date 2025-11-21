import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { DataTableComponent } from './components/table/data-table.component';
import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { FormFieldErrorComponent } from './components/form-field-error/form-field-error.component';
import { DateFormatPipe } from './pipes/date-format.pipe';
import { AutofocusDirective } from './directives/autofocus.directive';

@NgModule({
  declarations: [DataTableComponent, ConfirmDialogComponent, FormFieldErrorComponent, DateFormatPipe, AutofocusDirective],
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  exports: [CommonModule, FormsModule, ReactiveFormsModule, DataTableComponent, ConfirmDialogComponent, FormFieldErrorComponent, DateFormatPipe, AutofocusDirective],
})
export class SharedModule {}
