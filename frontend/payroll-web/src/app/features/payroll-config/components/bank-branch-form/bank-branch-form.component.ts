import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Bank } from '../../models/bank.model';
import { BankBranch } from '../../models/bank-branch.model';

@Component({
  selector: 'app-bank-branch-form',
  templateUrl: './bank-branch-form.component.html',
  styleUrls: ['./bank-branch-form.component.scss'],
})
export class BankBranchFormComponent implements OnChanges {
  @Input() initialValue: Partial<BankBranch> | null | undefined;
  @Input() banks: Bank[] = [];
  @Input() mode: 'create' | 'edit' | 'view' = 'create';
  @Output() submitted = new EventEmitter<Partial<BankBranch>>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      bankId: ['', Validators.required],
      code: ['', [Validators.required, Validators.maxLength(10), Validators.pattern(/^[A-Za-z0-9]+$/)]],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      isActive: [true],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && this.initialValue) {
      this.form.patchValue({
        bankId: this.initialValue.bankId ?? '',
        code: this.initialValue.code ?? '',
        name: this.initialValue.name ?? '',
        isActive: this.initialValue.isActive ?? true,
      });
    }

    if (changes['mode']) {
      if (this.mode === 'view') {
        this.form.disable({ emitEvent: false });
      } else {
        this.form.enable({ emitEvent: false });
      }
    }
  }

  submit(): void {
    if (this.mode === 'view') {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitted.emit(this.form.value);
  }
}
