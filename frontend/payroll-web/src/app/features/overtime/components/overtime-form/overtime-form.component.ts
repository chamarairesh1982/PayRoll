import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OTEntry, OvertimeType } from '../../models/ot-entry.model';

@Component({
  selector: 'app-overtime-form',
  templateUrl: './overtime-form.component.html',
  styleUrls: ['./overtime-form.component.scss'],
})
export class OvertimeFormComponent implements OnInit, OnChanges {
  @Input() initialValue?: Partial<OTEntry> | null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<OTEntry>>();

  form: FormGroup;
  overtimeTypes: OvertimeType[] = ['Normal', 'Weekend', 'Holiday'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      employeeId: ['', Validators.required],
      workDate: ['', Validators.required],
      rawMinutes: [0, [Validators.required, Validators.min(1)]],
      type: ['Normal', Validators.required],
      comment: [''],
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
