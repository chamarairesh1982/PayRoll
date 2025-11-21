import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employee-form',
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss'],
})
export class EmployeeFormComponent implements OnChanges {
  @Input() initialValue: Partial<Employee> | null | undefined;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<Employee>>();

  form: FormGroup;

  genders: Employee['gender'][] = ['Male', 'Female', 'Other'];
  maritalStatuses: Employee['maritalStatus'][] = ['Single', 'Married', 'Other'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      employeeCode: ['', Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      nicNumber: ['', [Validators.required, Validators.minLength(10)]],
      dateOfBirth: ['', Validators.required],
      gender: ['Male', Validators.required],
      maritalStatus: ['Single', Validators.required],
      employmentStartDate: ['', Validators.required],
      probationEndDate: [''],
      confirmationDate: [''],
      baseSalary: [0, [Validators.required, Validators.min(0.01)]],
      isActive: [true],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      this.form.patchValue(this.initialValue);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitted.emit(this.form.value);
  }
}
