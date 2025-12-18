import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { Employee } from '../../../employees/models/employee.model';
import { RecurringRule, RecurringRuleSimulationResult } from '../../models/recurring-rule.model';
import { RecurringRulesApiService } from '../../services/recurring-rules-api.service';
import { PaginatedResult } from '../../../employees/models/employee.model';

@Component({
  selector: 'app-recurring-rules-page',
  templateUrl: './recurring-rules-page.component.html',
  styleUrls: ['./recurring-rules-page.component.scss'],
})
export class RecurringRulesPageComponent implements OnInit {
  rules: RecurringRule[] = [];
  employees: Employee[] = [];
  simulationResults: RecurringRuleSimulationResult[] = [];
  form: FormGroup;
  page = 1;
  pageSize = 25;
  totalCount = 0;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';
  isSaving = false;
  isLoading = false;
  selectedRuleId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private employeesApi: EmployeesApiService,
    private recurringRulesApi: RecurringRulesApiService,
  ) {
    this.form = this.fb.group({
      code: ['', [Validators.required]],
      name: ['', [Validators.required]],
      ruleType: ['Allowance', Validators.required],
      frequency: ['Monthly', Validators.required],
      startDate: [this.todayAsInput(), Validators.required],
      endDate: [null],
      amount: [0, [Validators.required, Validators.min(0)]],
      employeeId: ['', Validators.required],
      isEpfApplicable: [true],
      isEtfApplicable: [true],
      isTaxable: [true],
      isActive: [true],
      periods: [3, [Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.loadRules();
    this.loadEmployees();
  }

  loadRules(): void {
    this.isLoading = true;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';

    this.recurringRulesApi
      .getRecurringRules({ page: this.page, pageSize: this.pageSize, isActive })
      .subscribe({
        next: (result: PaginatedResult<RecurringRule>) => {
          this.rules = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load recurring rules', err);
          this.isLoading = false;
        },
      });
  }

  loadEmployees(): void {
    this.employeesApi.getEmployees(1, 100).subscribe({
      next: res => (this.employees = res.items),
      error: err => console.error('Failed to load employees', err),
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.preparePayload();
    this.isSaving = true;

    const request$ = this.selectedRuleId
      ? this.recurringRulesApi.updateRecurringRule(this.selectedRuleId, payload)
      : this.recurringRulesApi.createRecurringRule(payload);

    request$.subscribe({
      next: () => {
        this.resetForm();
        this.loadRules();
        this.isSaving = false;
      },
      error: err => {
        console.error('Failed to save recurring rule', err);
        this.isSaving = false;
      },
    });
  }

  edit(rule: RecurringRule): void {
    this.selectedRuleId = rule.id;
    this.form.patchValue({
      code: rule.code,
      name: rule.name,
      ruleType: rule.ruleType,
      frequency: rule.frequency,
      startDate: rule.startDate,
      endDate: rule.endDate || null,
      amount: rule.amount,
      employeeId: rule.employeeId,
      isEpfApplicable: rule.isEpfApplicable,
      isEtfApplicable: rule.isEtfApplicable,
      isTaxable: rule.isTaxable,
      isActive: rule.isActive,
    });
  }

  delete(rule: RecurringRule): void {
    if (!confirm(`Delete rule ${rule.name}?`)) {
      return;
    }

    this.recurringRulesApi.deleteRecurringRule(rule.id).subscribe({
      next: () => this.loadRules(),
      error: err => console.error('Failed to delete recurring rule', err),
    });
  }

  resetForm(): void {
    this.selectedRuleId = null;
    this.simulationResults = [];
    this.form.reset({
      code: '',
      name: '',
      ruleType: 'Allowance',
      frequency: 'Monthly',
      startDate: this.todayAsInput(),
      endDate: null,
      amount: 0,
      employeeId: '',
      isEpfApplicable: true,
      isEtfApplicable: true,
      isTaxable: true,
      isActive: true,
      periods: 3,
    });
  }

  simulate(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.preparePayload();
    const periods = Number(this.form.get('periods')?.value || 1);

    this.recurringRulesApi
      .simulate({
        rule: {
          ...payload,
          frequency: payload.frequency || 'Monthly',
          ruleType: payload.ruleType || 'Allowance',
          startDate: payload.startDate as string,
          amount: payload.amount || 0,
          code: payload.code || '',
          name: payload.name || '',
          employeeId: payload.employeeId as string,
        },
        periods,
        startFrom: payload.startDate as string,
      })
      .subscribe({
        next: res => (this.simulationResults = res),
        error: err => console.error('Failed to simulate recurring rule', err),
      });
  }

  handleFilterChange(): void {
    this.page = 1;
    this.loadRules();
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.loadRules();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadRules();
    }
  }

  get totalPages(): number {
    return this.pageSize ? Math.ceil(this.totalCount / this.pageSize) : 1;
  }

  private preparePayload(): any {
    const value = this.form.value;
    return {
      code: value.code,
      name: value.name,
      ruleType: value.ruleType,
      frequency: value.frequency,
      startDate: value.startDate,
      endDate: value.endDate || null,
      amount: Number(value.amount),
      employeeId: value.employeeId,
      isEpfApplicable: value.isEpfApplicable,
      isEtfApplicable: value.isEtfApplicable,
      isTaxable: value.isTaxable,
      isActive: value.isActive,
    };
  }

  private todayAsInput(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
