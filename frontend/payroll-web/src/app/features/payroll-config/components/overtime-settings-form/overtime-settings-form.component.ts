import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OvertimeRuleConfig } from '../../models/overtime-rule-config.model';

@Component({
  selector: 'app-overtime-settings-form',
  templateUrl: './overtime-settings-form.component.html',
  styleUrls: ['./overtime-settings-form.component.scss'],
})
export class OvertimeSettingsFormComponent implements OnChanges {
  @Input() initialValue: OvertimeRuleConfig | null = null;
  @Input() isSaving = false;
  @Output() submitted = new EventEmitter<OvertimeRuleConfig>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      weekdayOvertimeMultiplier: [1.5, [Validators.required, Validators.min(0)]],
      weekendOvertimeMultiplier: [2, [Validators.required, Validators.min(0)]],
      holidayOvertimeMultiplier: [2, [Validators.required, Validators.min(0)]],
      overtimeRoundingMinutes: [15, [Validators.required, Validators.min(0)]],
      overtimeDailyCapHours: [12, [Validators.required, Validators.min(0)]],
      overtimePayRunCapHours: [80, [Validators.required, Validators.min(0)]],
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

    this.submitted.emit(this.form.value as OvertimeRuleConfig);
  }
}
