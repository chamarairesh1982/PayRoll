import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { Employee } from '../../../employees/models/employee.model';
import { RecurringScopeType } from '../../models/recurring-rule.model';

@Component({
  selector: 'app-scope-picker',
  templateUrl: './scope-picker.component.html',
  styleUrls: ['./scope-picker.component.scss'],
})
export class ScopePickerComponent {
  @Input({ required: true }) form!: FormGroup;
  @Input() companies: CompanyOption[] = [];
  @Input() branches: BranchOption[] = [];
  @Input() costCenters: CostCenterOption[] = [];
  @Input() employees: Employee[] = [];
  @Input() isReadOnly = false;

  @Output() companyChange = new EventEmitter<string | null>();
  @Output() branchChange = new EventEmitter<string | null>();

  scopeOptions = [
    { label: 'All employees', value: 'All' as RecurringScopeType },
    { label: 'Filtered group', value: 'Group' as RecurringScopeType },
    { label: 'Selected employees', value: 'Selected' as RecurringScopeType },
  ];

  categoryOptions = [
    { label: 'Permanent', value: 'Permanent' },
    { label: 'Contract', value: 'Contract' },
    { label: 'Intern', value: 'Intern' },
  ];

  get scopeType(): RecurringScopeType {
    return this.form.get('scopeType')?.value as RecurringScopeType;
  }

  get employeeOptions(): { label: string; value: string }[] {
    return this.employees.map(employee => ({
      label: `${employee.firstName} ${employee.lastName}`,
      value: employee.id,
    }));
  }

  onCompanyChange(value: string | null): void {
    this.companyChange.emit(value);
  }

  onBranchChange(value: string | null): void {
    this.branchChange.emit(value);
  }
}
