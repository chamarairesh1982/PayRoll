import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TaxRuleSet, TaxSlab } from '../../models/tax-rule-set.model';

@Component({
  selector: 'app-tax-rule-set-form',
  templateUrl: './tax-rule-set-form.component.html',
  styleUrls: ['./tax-rule-set-form.component.scss'],
})
export class TaxRuleSetFormComponent implements OnChanges {
  @Input() initialValue: Partial<TaxRuleSet> | null = null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<TaxRuleSet>>();

  form: FormGroup;
  slabs: TaxSlab[] = [];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      yearOfAssessment: [new Date().getFullYear(), [Validators.required, Validators.min(1900)]],
      effectiveFrom: ['', Validators.required],
      effectiveTo: [''],
      isDefault: [false],
      isActive: [true],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      const { slabs = [], ...rest } = this.initialValue;
      const patch = {
        ...rest,
        effectiveFrom: this.initialValue.effectiveFrom?.split('T')[0] || this.initialValue.effectiveFrom,
        effectiveTo: this.initialValue.effectiveTo ? this.initialValue.effectiveTo.split('T')[0] : '',
      };
      this.form.patchValue(patch);
      this.slabs = [...slabs].sort((a, b) => a.order - b.order);
    }
  }

  updateSlabs(slabs: TaxSlab[]): void {
    this.slabs = slabs;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value;
    this.submitted.emit({
      ...value,
      yearOfAssessment: Number(value.yearOfAssessment),
      slabs: this.slabs.map(slab => ({
        ...slab,
        fromAmount: Number(slab.fromAmount),
        toAmount: slab.toAmount === null || slab.toAmount === undefined || slab.toAmount === '' ? null : Number(slab.toAmount),
        ratePercent: Number(slab.ratePercent),
        order: slab.order,
      })),
      effectiveTo: value.effectiveTo ? value.effectiveTo : null,
    });
  }
}
