import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OTRule, OTRulePayload } from '../../models/ot-rule.model';

@Component({
  selector: 'app-overtime-settings-form',
  templateUrl: './overtime-settings-form.component.html',
  styleUrls: ['./overtime-settings-form.component.scss'],
})
export class OvertimeSettingsFormComponent implements OnChanges {
  @Input() initialValue: OTRule | null = null;
  @Input() isSaving = false;
  @Output() submitted = new EventEmitter<OTRulePayload>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(150)]],
      weekdayMultiplier: [1.5, [Validators.required, Validators.min(0)]],
      weekendMultiplier: [2, [Validators.required, Validators.min(0)]],
      holidayMultiplier: [2, [Validators.required, Validators.min(0)]],
      roundingMinutes: [15, [Validators.required, Validators.min(0)]],
      dailyCapHours: [12, [Validators.required, Validators.min(0)]],
      payRunCapHours: [80, [Validators.required, Validators.min(0)]],
      appliesOnWeekend: [true],
      appliesOnHoliday: [true],
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

    this.submitted.emit(this.form.value as OTRulePayload);
  }
}
