import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AllowanceType, CalculationBasis } from '../../models/allowance-type.model';

@Component({
  selector: 'app-allowance-type-form',
  templateUrl: './allowance-type-form.component.html',
  styleUrls: ['./allowance-type-form.component.scss'],
})
export class AllowanceTypeFormComponent implements OnChanges {
  @Input() initialValue: Partial<AllowanceType> | null | undefined;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<AllowanceType>>();

  form: FormGroup;
  basisOptions: CalculationBasis[] = ['FixedAmount', 'PercentageOfBasic', 'PercentageOfGross', 'PerDay', 'PerHour'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      code: ['', [Validators.required, Validators.pattern(/^[A-Z0-9_]+$/)]],
      name: ['', Validators.required],
      description: [''],
      basis: ['FixedAmount', Validators.required],
      isEpfApplicable: [false],
      isEtfApplicable: [false],
      isTaxable: [false],
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

    this.submitted.emit(this.form.value);
  }
}
