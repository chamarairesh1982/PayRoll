import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Employee, PaginatedResult } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { AllowanceType } from '../../../payroll-config/models/allowance-type.model';
import { DeductionType } from '../../../payroll-config/models/deduction-type.model';
import { AllowanceTypesApiService } from '../../../payroll-config/services/allowance-types-api.service';
import { DeductionTypesApiService } from '../../../payroll-config/services/deduction-types-api.service';
import {
  RecurringPayItemAssignment,
  RecurringPayItemRule,
  RecurringPayItemSimulationResponse,
} from '../../models/recurring-pay-item.model';
import { RecurringPayItemsApiService } from '../../services/recurring-pay-items-api.service';

@Component({
  selector: 'app-recurring-rules-page',
  templateUrl: './recurring-rules-page.component.html',
  styleUrls: ['./recurring-rules-page.component.scss'],
})
export class RecurringRulesPageComponent implements OnInit {
  rules: RecurringPayItemRule[] = [];
  assignments: RecurringPayItemAssignment[] = [];
  employees: Employee[] = [];
  allowances: AllowanceType[] = [];
  deductions: DeductionType[] = [];
  simulationResult?: RecurringPayItemSimulationResponse;
  ruleForm: FormGroup;
  assignmentForm: FormGroup;
  previewForm: FormGroup;
  page = 1;
  pageSize = 25;
  totalCount = 0;
  filterIsActive: 'all' | 'active' | 'inactive' = 'active';
  isSaving = false;
  isLoading = false;
  isAssignmentsLoading = false;
  isPreviewLoading = false;
  loadError: string | null = null;
  assignmentError: string | null = null;
  previewError: string | null = null;
  selectedRuleId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private employeesApi: EmployeesApiService,
    private allowanceTypesApi: AllowanceTypesApiService,
    private deductionTypesApi: DeductionTypesApiService,
    private recurringPayItemsApi: RecurringPayItemsApiService,
  ) {
    this.ruleForm = this.fb.group({
      name: ['', [Validators.required]],
      ruleType: ['Allowance', Validators.required],
      payComponentId: ['', Validators.required],
      frequency: ['Monthly', Validators.required],
      startDate: [this.todayAsInput(), Validators.required],
      endDate: [null],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      taxable: [true],
      epfEtfContributable: [true],
      prorate: [false],
      isActive: [true],
    });

    this.assignmentForm = this.fb.group({
      employeeIds: [[], Validators.required],
      startDate: [this.todayAsInput(), Validators.required],
      endDate: [null],
      isActive: [true],
    });

    this.previewForm = this.fb.group({
      employeeId: ['', Validators.required],
      periodStart: [this.todayAsInput(), Validators.required],
      periodEnd: [this.todayAsInput(), Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadRules();
    this.loadEmployees();
    this.loadPayComponents();
  }

  loadRules(): void {
    this.isLoading = true;
    this.loadError = null;
    const isActive = this.filterIsActive === 'all' ? null : this.filterIsActive === 'active';

    this.recurringPayItemsApi
      .getRules({ page: this.page, pageSize: this.pageSize, activeOnly: isActive ?? undefined })
      .subscribe({
        next: (result: PaginatedResult<RecurringPayItemRule>) => {
          this.rules = result.items;
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load recurring rules', err);
          this.loadError = 'Failed to load recurring rules.';
          this.isLoading = false;
        },
      });
  }

  loadEmployees(): void {
    this.employeesApi.getEmployees(1, 200).subscribe({
      next: res => (this.employees = res.items),
      error: err => console.error('Failed to load employees', err),
    });
  }

  loadPayComponents(): void {
    this.allowanceTypesApi.getAllowanceTypes({ page: 1, pageSize: 200, isActive: true }).subscribe({
      next: res => (this.allowances = res.items),
      error: err => console.error('Failed to load allowance types', err),
    });

    this.deductionTypesApi.getDeductionTypes({ page: 1, pageSize: 200, isActive: true }).subscribe({
      next: res => (this.deductions = res.items),
      error: err => console.error('Failed to load deduction types', err),
    });
  }

  submitRule(): void {
    if (this.ruleForm.invalid) {
      this.ruleForm.markAllAsTouched();
      return;
    }

    const payload = this.prepareRulePayload();
    this.isSaving = true;

    const request$ = this.selectedRuleId
      ? this.recurringPayItemsApi.updateRule(this.selectedRuleId, payload)
      : this.recurringPayItemsApi.createRule(payload);

    request$.subscribe({
      next: () => {
        this.resetRuleForm();
        this.loadRules();
        this.isSaving = false;
      },
      error: err => {
        console.error('Failed to save recurring rule', err);
        this.isSaving = false;
      },
    });
  }

  edit(rule: RecurringPayItemRule): void {
    this.selectedRuleId = rule.id;
    this.ruleForm.patchValue({
      name: rule.name,
      ruleType: rule.ruleType,
      payComponentId: rule.payComponentId,
      frequency: rule.frequency,
      startDate: rule.startDate,
      endDate: rule.endDate || null,
      amount: rule.amount,
      taxable: rule.taxable,
      epfEtfContributable: rule.epfEtfContributable,
      prorate: rule.prorate,
      isActive: rule.isActive,
    });
    this.loadAssignments();
  }

  delete(rule: RecurringPayItemRule): void {
    if (!confirm(`Delete rule ${rule.name}?`)) {
      return;
    }

    this.recurringPayItemsApi.deleteRule(rule.id).subscribe({
      next: () => this.loadRules(),
      error: err => console.error('Failed to delete recurring rule', err),
    });
  }

  resetRuleForm(): void {
    this.selectedRuleId = null;
    this.assignments = [];
    this.simulationResult = undefined;
    this.ruleForm.reset({
      name: '',
      ruleType: 'Allowance',
      payComponentId: '',
      frequency: 'Monthly',
      startDate: this.todayAsInput(),
      endDate: null,
      amount: 0,
      taxable: true,
      epfEtfContributable: true,
      prorate: false,
      isActive: true,
    });

    this.assignmentForm.reset({
      employeeIds: [],
      startDate: this.todayAsInput(),
      endDate: null,
      isActive: true,
    });
  }

  submitAssignments(): void {
    if (!this.selectedRuleId) {
      this.assignmentError = 'Select a rule to assign employees.';
      return;
    }

    if (this.assignmentForm.invalid) {
      this.assignmentForm.markAllAsTouched();
      return;
    }

    const payload = this.assignmentForm.value;
    const employeeIds = payload.employeeIds as string[];
    if (!employeeIds.length) {
      this.assignmentError = 'Select at least one employee.';
      return;
    }

    this.assignmentError = null;
    this.isAssignmentsLoading = true;

    this.recurringPayItemsApi
      .createAssignments({
        ruleId: this.selectedRuleId,
        employeeIds,
        startDate: payload.startDate,
        endDate: payload.endDate || null,
        isActive: payload.isActive,
      })
      .subscribe({
        next: () => {
          this.assignmentForm.patchValue({ employeeIds: [] });
          this.loadAssignments();
          this.isAssignmentsLoading = false;
        },
        error: err => {
          console.error('Failed to create assignments', err);
          this.assignmentError = 'Failed to create assignments.';
          this.isAssignmentsLoading = false;
        },
      });
  }

  loadAssignments(): void {
    if (!this.selectedRuleId) {
      this.assignments = [];
      return;
    }

    this.isAssignmentsLoading = true;
    this.assignmentError = null;

    this.recurringPayItemsApi
      .getAssignments({ ruleId: this.selectedRuleId, activeOnly: true })
      .subscribe({
        next: res => {
          this.assignments = res;
          this.isAssignmentsLoading = false;
        },
        error: err => {
          console.error('Failed to load assignments', err);
          this.assignmentError = 'Failed to load assignments.';
          this.isAssignmentsLoading = false;
        },
      });
  }

  removeAssignment(assignment: RecurringPayItemAssignment): void {
    if (!confirm(`Remove assignment for ${assignment.employeeName}?`)) {
      return;
    }

    this.recurringPayItemsApi.deleteAssignment(assignment.id).subscribe({
      next: () => this.loadAssignments(),
      error: err => console.error('Failed to remove assignment', err),
    });
  }

  preview(): void {
    if (this.previewForm.invalid) {
      this.previewForm.markAllAsTouched();
      return;
    }

    this.previewError = null;
    this.isPreviewLoading = true;
    const payload = this.previewForm.value;

    this.recurringPayItemsApi
      .simulate({
        employeeId: payload.employeeId,
        periodStart: payload.periodStart,
        periodEnd: payload.periodEnd,
      })
      .subscribe({
        next: res => {
          this.simulationResult = res;
          this.isPreviewLoading = false;
        },
        error: err => {
          console.error('Failed to simulate recurring items', err);
          this.previewError = 'Failed to simulate recurring items.';
          this.isPreviewLoading = false;
        },
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

  get payComponentOptions(): { id: string; label: string }[] {
    const ruleType = this.ruleForm.get('ruleType')?.value;
    if (ruleType === 'Deduction') {
      return this.deductions.map(item => ({ id: item.id, label: `${item.code} - ${item.name}` }));
    }
    return this.allowances.map(item => ({ id: item.id, label: `${item.code} - ${item.name}` }));
  }

  private prepareRulePayload(): any {
    const value = this.ruleForm.value;
    return {
      name: value.name,
      ruleType: value.ruleType,
      payComponentId: value.payComponentId,
      frequency: value.frequency,
      startDate: value.startDate,
      endDate: value.endDate || null,
      amount: Number(value.amount),
      taxable: value.taxable,
      epfEtfContributable: value.epfEtfContributable,
      prorate: value.prorate,
      isActive: value.isActive,
    };
  }

  private todayAsInput(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
