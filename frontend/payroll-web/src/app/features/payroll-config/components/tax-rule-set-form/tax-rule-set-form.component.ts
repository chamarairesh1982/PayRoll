import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TaxRelief, TaxReliefFrequency, TaxReliefType, TaxRuleSet, TaxSlab } from '../../models/tax-rule-set.model';

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
  reliefs: TaxRelief[] = [];
  reliefTypeOptions: { label: string; value: TaxReliefType }[] = [
    { label: 'Income relief', value: 'IncomeRelief' },
    { label: 'Tax rebate', value: 'TaxRebate' },
  ];
  reliefFrequencyOptions: { label: string; value: TaxReliefFrequency }[] = [
    { label: 'Monthly', value: 'Monthly' },
    { label: 'Annual', value: 'Annual' },
  ];

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
      const { slabs = [], reliefs = [], ...rest } = this.initialValue;
      const patch = {
        ...rest,
        effectiveFrom: this.initialValue.effectiveFrom?.split('T')[0] || this.initialValue.effectiveFrom,
        effectiveTo: this.initialValue.effectiveTo ? this.initialValue.effectiveTo.split('T')[0] : '',
      };
      this.form.patchValue(patch);
      this.slabs = [...slabs].sort((a, b) => a.order - b.order);
      this.reliefs = [...reliefs];
    }
  }

  updateSlabs(slabs: TaxSlab[]): void {
    this.slabs = slabs;
  }

  addRelief(): void {
    this.reliefs = [
      ...this.reliefs,
      {
        name: '',
        amount: 0,
        reliefType: 'IncomeRelief',
        frequency: 'Monthly',
      },
    ];
  }

  updateRelief(index: number, changes: Partial<TaxRelief>): void {
    this.reliefs = this.reliefs.map((relief, i) => (i === index ? { ...relief, ...changes } : relief));
  }

  removeRelief(index: number): void {
    this.reliefs = this.reliefs.filter((_, i) => i !== index);
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
        toAmount: slab.toAmount === null || slab.toAmount === undefined ? null : Number(slab.toAmount),
        ratePercent: Number(slab.ratePercent),
        order: slab.order,
      })),
      reliefs: this.reliefs.map(relief => ({
        ...relief,
        amount: Number(relief.amount),
      })),
      effectiveTo: value.effectiveTo ? value.effectiveTo : null,
    });
  }
}
