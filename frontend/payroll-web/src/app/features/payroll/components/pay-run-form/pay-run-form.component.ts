import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Employee } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { PayPeriodType, PayRunSummary } from '../../models/pay-run.model';

@Component({
  selector: 'app-pay-run-form',
  templateUrl: './pay-run-form.component.html',
  styleUrls: ['./pay-run-form.component.scss'],
})
export class PayRunFormComponent implements OnInit, OnChanges {
  @Input() initialValue?: Partial<PayRunSummary> | null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<{
    name: string;
    periodType: PayPeriodType;
    periodStart: string;
    periodEnd: string;
    payDate: string;
    includeActiveEmployeesOnly: boolean;
    employeeIds?: string[];
  }>();

  form: FormGroup;
  periodTypes: PayPeriodType[] = ['Monthly', 'Weekly', 'Custom'];

  employees: Employee[] = [];
  filteredEmployees: Employee[] = [];
  loadingEmployees = true;
  employeesLoadError: string | null = null;
  employeeSearch = '';

  constructor(private fb: FormBuilder, private employeesApi: EmployeesApiService) {
    this.form = this.fb.group(
      {
        name: ['', Validators.required],
        periodType: ['Monthly', Validators.required],
        periodStart: ['', Validators.required],
        periodEnd: ['', Validators.required],
        payDate: ['', Validators.required],
        includeActiveEmployeesOnly: [true],
        employeeIds: [[]],
      },
      { validators: this.periodRangeValidator },
    );
  }

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue);
    }

    this.loadEmployees();

    this.form.get('includeActiveEmployeesOnly')?.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && changes['initialValue'].currentValue) {
      this.form.patchValue(changes['initialValue'].currentValue);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value as {
      name: string;
      periodType: PayPeriodType;
      periodStart: string;
      periodEnd: string;
      payDate: string;
      includeActiveEmployeesOnly: boolean;
      employeeIds?: string[];
    };

    const employeeIds = value.employeeIds || [];

    this.submitted.emit({
      name: value.name,
      periodType: value.periodType,
      periodStart: value.periodStart,
      periodEnd: value.periodEnd,
      payDate: value.payDate,
      includeActiveEmployeesOnly: value.includeActiveEmployeesOnly,
      ...(employeeIds && employeeIds.length ? { employeeIds } : {}),
    });
  }

  onEmployeeSearchChange(term: string): void {
    this.employeeSearch = term;
    this.applyFilters();
  }

  private loadEmployees(): void {
    this.loadingEmployees = true;
    this.employeesLoadError = null;
    this.employeesApi.getEmployees(1, 1000).subscribe({
      next: result => {
        this.employees = result.items;
        this.loadingEmployees = false;
        this.applyFilters();
      },
      error: err => {
        console.error('Failed to load employees', err);
        this.employeesLoadError = 'Failed to load employees. Please try again later.';
        this.loadingEmployees = false;
        this.filteredEmployees = [];
      },
    });
  }

  private applyFilters(): void {
    const includeActiveOnly = this.form.get('includeActiveEmployeesOnly')?.value;
    const search = this.employeeSearch.trim().toLowerCase();

    let results = this.employees;

    if (includeActiveOnly) {
      results = results.filter(emp => emp.isActive);
    }

    if (search) {
      results = results.filter(emp => {
        const fields = [
          emp.firstName,
          emp.lastName,
          emp.employeeCode,
          emp.callingName ?? '',
          emp.initials ?? '',
        ]
          .filter(Boolean)
          .map(val => val.toLowerCase());

        return fields.some(field => field.includes(search));
      });
    }

    this.filteredEmployees = results;
  }

  private periodRangeValidator = (group: FormGroup) => {
    const start = group.get('periodStart')?.value;
    const end = group.get('periodEnd')?.value;

    if (start && end && new Date(end) < new Date(start)) {
      return { periodRange: true };
    }

    return null;
  };

  get periodRangeInvalid(): boolean {
    return !!this.form.errors?.['periodRange'] && this.form.get('periodEnd')?.touched;
  }
}
