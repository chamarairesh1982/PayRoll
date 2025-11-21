import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OvertimeRecord, OvertimeStatus, OvertimeType } from '../../models/overtime-record.model';

@Component({
  selector: 'app-overtime-form',
  templateUrl: './overtime-form.component.html',
  styleUrls: ['./overtime-form.component.scss'],
})
export class OvertimeFormComponent implements OnInit, OnChanges {
  @Input() initialValue?: Partial<OvertimeRecord> | null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<OvertimeRecord>>();

  form: FormGroup;
  overtimeTypes: OvertimeType[] = ['Weekday', 'Weekend', 'PublicHoliday'];
  statuses: OvertimeStatus[] = ['Pending', 'Approved', 'Rejected', 'Cancelled'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      employeeId: ['', Validators.required],
      date: ['', Validators.required],
      hours: [0, [Validators.required, Validators.min(0.01)]],
      type: ['Weekday', Validators.required],
      status: ['Pending'],
      reason: [''],
    });
  }

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue']?.currentValue) {
      this.form.patchValue(changes['initialValue'].currentValue);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitted.emit(this.form.value);
  }
}
