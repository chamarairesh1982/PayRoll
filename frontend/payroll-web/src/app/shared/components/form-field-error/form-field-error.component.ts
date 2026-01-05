import { Component, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-form-field-error',
  templateUrl: './form-field-error.component.html',
  styleUrls: ['./form-field-error.component.scss'],
})
export class FormFieldErrorComponent {
  @Input() control: AbstractControl | null = null;

  get hasError(): boolean {
    return !!this.control && this.control.invalid && (this.control.dirty || this.control.touched);
  }

  get errors(): string[] {
    if (!this.control || !this.control.errors) {
      return [];
    }
    return Object.keys(this.control.errors).map(key => {
      switch (key) {
        case 'required':
          return 'This field is required';
        case 'min':
          return 'Value is too low';
        case 'minlength':
          return 'Value is too short';
        default:
          return 'Invalid value';
      }
    });
  }
}
