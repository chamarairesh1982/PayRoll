import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { Employee } from '../../../employees/models/employee.model';
import {
  RecurringAmountType,
  RecurringFrequency,
  RecurringRuleStatus,
  RecurringRuleType,
} from '../../models/recurring-rule.model';

@Component({
  selector: 'app-rule-form',
  templateUrl: './rule-form.component.html',
  styleUrls: ['./rule-form.component.scss'],
})
export class RuleFormComponent {
  @Input({ required: true }) form!: FormGroup;
  @Input() companies: CompanyOption[] = [];
  @Input() branches: BranchOption[] = [];
  @Input() costCenters: CostCenterOption[] = [];
  @Input() employees: Employee[] = [];
  @Input() isReadOnly = false;

  @Output() companyChange = new EventEmitter<string | null>();
  @Output() branchChange = new EventEmitter<string | null>();

  typeOptions: { label: string; value: RecurringRuleType }[] = [
    { label: 'Allowance (Earning)', value: 'Allowance' },
    { label: 'Deduction', value: 'Deduction' },
    { label: 'Loan installment', value: 'Loan installment' },
    { label: 'Overtime rule', value: 'Overtime rule' },
  ];

  statusOptions: { label: string; value: RecurringRuleStatus }[] = [
    { label: 'Active', value: 'Active' },
    { label: 'Inactive', value: 'Inactive' },
  ];

  frequencyOptions: { label: string; value: RecurringFrequency }[] = [
    { label: 'Monthly', value: 'Monthly' },
    { label: 'Weekly', value: 'Weekly' },
    { label: 'Fortnightly', value: 'Fortnightly' },
  ];

  amountTypeOptions: { label: string; value: RecurringAmountType }[] = [
    { label: 'Fixed amount', value: 'Fixed' },
    { label: '% of base salary', value: 'Percentage' },
  ];

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  handleCompanyChange(value: string | null): void {
    this.companyChange.emit(value);
  }

  handleBranchChange(value: string | null): void {
    this.branchChange.emit(value);
  }
}
