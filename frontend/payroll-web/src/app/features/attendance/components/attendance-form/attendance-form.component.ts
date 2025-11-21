import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AttendanceRecord, AttendanceStatus } from '../../models/attendance-record.model';

@Component({
  selector: 'app-attendance-form',
  templateUrl: './attendance-form.component.html',
  styleUrls: ['./attendance-form.component.scss'],
})
export class AttendanceFormComponent implements OnInit, OnChanges {
  @Input() initialValue?: Partial<AttendanceRecord> | null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<AttendanceRecord>>();

  form: FormGroup;
  statuses: AttendanceStatus[] = ['Present', 'Absent', 'Leave', 'HalfDay'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      date: ['', Validators.required],
      employeeId: ['', Validators.required],
      inTime: [''],
      outTime: [''],
      status: ['Present', Validators.required],
      otHours: [0, [Validators.min(0)]],
      remarks: [''],
    });
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

    // TODO: add validation to ensure outTime is after inTime when both are provided.
    this.submitted.emit(this.form.value);
  }
}
