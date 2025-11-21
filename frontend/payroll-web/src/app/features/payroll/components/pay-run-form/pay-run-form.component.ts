import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group(
      {
        name: ['', Validators.required],
        periodType: ['Monthly', Validators.required],
        periodStart: ['', Validators.required],
        periodEnd: ['', Validators.required],
        payDate: ['', Validators.required],
        includeActiveEmployeesOnly: [true],
        employeeIds: [''],
      },
      { validators: this.periodRangeValidator },
    );
  }

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue);
    }
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
      employeeIds?: string;
    };

    const employeeIds = value.employeeIds
      ? value.employeeIds
          .split(/[\s,]+/)
          .map(id => id.trim())
          .filter(Boolean)
      : undefined;

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
