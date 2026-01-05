import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EpfEtfRuleSet } from '../../models/epf-etf-rule-set.model';

@Component({
  selector: 'app-epf-etf-rule-set-form',
  templateUrl: './epf-etf-rule-set-form.component.html',
  styleUrls: ['./epf-etf-rule-set-form.component.scss'],
})
export class EpfEtfRuleSetFormComponent implements OnChanges {
  @Input() initialValue: Partial<EpfEtfRuleSet> | null = null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<EpfEtfRuleSet>>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      effectiveFrom: ['', Validators.required],
      effectiveTo: [''],
      employeeEpfRate: [0, [Validators.required, Validators.min(0)]],
      employerEpfRate: [0, [Validators.required, Validators.min(0)]],
      employerEtfRate: [0, [Validators.required, Validators.min(0)]],
      minimumWageForEpf: [''],
      maximumEarningForEpf: [''],
      maximumEarningForEtf: [''],
      isDefault: [false],
      isActive: [true],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      const patch = {
        ...this.initialValue,
        effectiveFrom: this.initialValue.effectiveFrom?.split('T')[0] || this.initialValue.effectiveFrom,
        effectiveTo: this.initialValue.effectiveTo ? this.initialValue.effectiveTo.split('T')[0] : '',
      };
      this.form.patchValue(patch);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    this.submitted.emit({
      ...value,
      employeeEpfRate: Number(value.employeeEpfRate),
      employerEpfRate: Number(value.employerEpfRate),
      employerEtfRate: Number(value.employerEtfRate),
      minimumWageForEpf: value.minimumWageForEpf === '' ? null : Number(value.minimumWageForEpf),
      maximumEarningForEpf: value.maximumEarningForEpf === '' ? null : Number(value.maximumEarningForEpf),
      maximumEarningForEtf: value.maximumEarningForEtf === '' ? null : Number(value.maximumEarningForEtf),
      effectiveTo: value.effectiveTo ? value.effectiveTo : null,
    });
  }
}
