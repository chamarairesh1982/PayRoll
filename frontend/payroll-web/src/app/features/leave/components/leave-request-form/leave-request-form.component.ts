import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { LeaveRequest, LeaveTypeCode } from '../../models/leave-request.model';

@Component({
  selector: 'app-leave-request-form',
  templateUrl: './leave-request-form.component.html',
  styleUrls: ['./leave-request-form.component.scss'],
})
export class LeaveRequestFormComponent implements OnInit, OnChanges {
  @Input() initialValue?: Partial<LeaveRequest> | null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<LeaveRequest>>();

  form: FormGroup;
  leaveTypes: LeaveTypeCode[] = ['ANNUAL', 'CASUAL', 'SICK', 'MATERNITY', 'NOPAY', 'OTHER'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group(
      {
        employeeId: ['', Validators.required],
        leaveType: ['ANNUAL', Validators.required],
        startDate: ['', Validators.required],
        endDate: ['', Validators.required],
        isHalfDay: [false],
        halfDaySession: [''],
        reason: [''],
      },
      { validators: this.dateRangeValidator }
    );

    this.form.get('isHalfDay')?.valueChanges.subscribe(() => this.updateHalfDayValidators());
  }

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue);
      this.updateHalfDayValidators();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && changes['initialValue'].currentValue) {
      this.form.patchValue(changes['initialValue'].currentValue);
      this.updateHalfDayValidators();
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitted.emit(this.form.value);
  }

  private dateRangeValidator(control: AbstractControl): ValidationErrors | null {
    const start = control.get('startDate')?.value;
    const end = control.get('endDate')?.value;

    if (start && end && end < start) {
      return { endBeforeStart: true };
    }

    return null;
  }

  private updateHalfDayValidators(): void {
    const halfDayControl = this.form.get('halfDaySession');
    const isHalfDay = this.form.get('isHalfDay')?.value;

    if (isHalfDay) {
      halfDayControl?.setValidators([Validators.required]);
    } else {
      halfDayControl?.clearValidators();
      halfDayControl?.setValue('');
    }

    halfDayControl?.updateValueAndValidity({ emitEvent: false });
  }
}
