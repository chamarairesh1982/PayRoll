import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { CalculationBasis } from '../../models/allowance-type.model';
import { DeductionType } from '../../models/deduction-type.model';

@Component({
  selector: 'app-deduction-type-form',
  templateUrl: './deduction-type-form.component.html',
  styleUrls: ['./deduction-type-form.component.scss'],
})
export class DeductionTypeFormComponent implements OnChanges {
  @Input() initialValue: Partial<DeductionType> | null | undefined;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<DeductionType>>();

  form: FormGroup;
  basisOptions: CalculationBasis[] = ['FixedAmount', 'PercentageOfBasic', 'PercentageOfGross', 'PerDay', 'PerHour'];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group(
      {
        code: ['', [Validators.required, Validators.pattern(/^[A-Z0-9_]+$/)]],
        name: ['', Validators.required],
        description: [''],
        basis: ['FixedAmount', Validators.required],
        isPreTax: [false],
        isPostTax: [false],
        isActive: [true],
      },
      { validators: this.requireAtLeastOneTaxType() }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      this.form.patchValue(this.initialValue);
      this.form.updateValueAndValidity({ emitEvent: false });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitted.emit(this.form.value);
  }

  private requireAtLeastOneTaxType(): ValidatorFn {
    return (control: AbstractControl) => {
      const isPreTax = control.get('isPreTax')?.value;
      const isPostTax = control.get('isPostTax')?.value;

      return isPreTax || isPostTax ? null : { taxTypeRequired: true };
    };
  }
}
