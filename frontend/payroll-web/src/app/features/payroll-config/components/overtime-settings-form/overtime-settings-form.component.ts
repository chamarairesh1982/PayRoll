import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OTRule, OTRulePayload, OvertimeRoundingMode, OvertimeType } from '../../models/ot-rule.model';

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
  overtimeTypes: OvertimeType[] = ['Normal', 'Weekend', 'Holiday'];
  roundingModes: OvertimeRoundingMode[] = ['Down', 'Nearest', 'Up'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      type: ['Normal', [Validators.required]],
      multiplier: [1.5, [Validators.required, Validators.min(0.01)]],
      roundToMinutes: [15, [Validators.required, Validators.min(1)]],
      roundingMode: ['Nearest', [Validators.required]],
      dailyHoursCap: [null, [Validators.min(0)]],
      monthlyHoursCap: [null, [Validators.min(0)]],
      effectiveFrom: ['', [Validators.required]],
      effectiveTo: [null],
      isActive: [true],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      const formValue = {
        ...this.initialValue,
        effectiveFrom: this.initialValue.effectiveFrom ? new Date(this.initialValue.effectiveFrom) : null,
        effectiveTo: this.initialValue.effectiveTo ? new Date(this.initialValue.effectiveTo) : null,
      };
      this.form.patchValue(formValue);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.value;
    const payload: OTRulePayload = {
      ...formValue,
      effectiveFrom: formValue.effectiveFrom instanceof Date
        ? formValue.effectiveFrom.toISOString().split('T')[0]
        : formValue.effectiveFrom,
      effectiveTo: formValue.effectiveTo instanceof Date
        ? formValue.effectiveTo.toISOString().split('T')[0]
        : formValue.effectiveTo,
    };

    this.submitted.emit(payload);
  }
}
