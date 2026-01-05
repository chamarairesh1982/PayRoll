import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Subject, takeUntil } from 'rxjs';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { Employee } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import {
  RecurringRule,
  RecurringRuleFormValue,
  RecurringRuleStatus,
} from '../../models/recurring-rule.model';
import { RecurringRuleService } from '../../services/recurring-rule.service';

@Component({
  selector: 'app-recurring-rule-form-page',
  templateUrl: './recurring-rule-form-page.component.html',
  styleUrls: ['./recurring-rule-form-page.component.scss'],
  providers: [MessageService],
})
export class RecurringRuleFormPageComponent implements OnInit, OnDestroy {
  form: FormGroup;
  isEditMode = false;
  isViewMode = false;
  ruleId: string | null = null;
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];
  employees: Employee[] = [];

  sampleSalary = 100000;
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private recurringRuleService: RecurringRuleService,
    private organizationApi: OrganizationApiService,
    private employeesApi: EmployeesApiService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService,
  ) {
    this.form = this.fb.group(
      {
        name: ['', [Validators.required, Validators.maxLength(80)]],
        type: ['Allowance', Validators.required],
        status: ['Active' as RecurringRuleStatus, Validators.required],
        priority: [1, [Validators.required, Validators.min(1)]],
        frequency: ['Monthly', Validators.required],
        amountType: ['Fixed', Validators.required],
        amountValue: [0, [Validators.required, Validators.min(0.01)]],
        scopeType: ['All', Validators.required],
        companyId: [null],
        branchId: [null],
        costCenterId: [null],
        employeeCategory: [null],
        employeeIds: [[]],
        startDate: [this.todayAsInput(), Validators.required],
        endDate: [null],
      },
      {
        validators: [this.dateRangeValidator],
      },
    );
  }

  ngOnInit(): void {
    this.ruleId = this.route.snapshot.paramMap.get('id');
    this.isViewMode = this.route.snapshot.routeConfig?.path === ':id';
    this.isEditMode = !!this.ruleId && !this.isViewMode;

    this.loadOrganizations();
    this.loadEmployees();

    if (this.ruleId) {
      this.loadRule(this.ruleId);
    }

    this.form
      .get('amountType')
      ?.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(value => this.updateAmountValidators(value));

    this.form
      .get('scopeType')
      ?.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => this.updateScopeDefaults());

    if (this.isViewMode) {
      this.form.disable({ emitEvent: false });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadOrganizations(): void {
    this.organizationApi.getCompanies().subscribe(companies => {
      this.companies = companies;
      this.loadBranches();
      this.loadCostCenters();
    });
  }

  loadBranches(): void {
    const companyId = this.form.get('companyId')?.value;
    this.organizationApi.getBranches(companyId || undefined).subscribe(branches => {
      this.branches = branches;
    });
  }

  loadCostCenters(): void {
    const companyId = this.form.get('companyId')?.value;
    const branchId = this.form.get('branchId')?.value;
    this.organizationApi.getCostCenters(companyId || undefined, branchId || undefined).subscribe(costCenters => {
      this.costCenters = costCenters;
    });
  }

  loadEmployees(): void {
    this.employeesApi.getEmployees(1, 200).subscribe(result => {
      this.employees = result.items;
    });
  }

  loadRule(id: string): void {
    this.recurringRuleService.getRuleById(id).subscribe(rule => {
      if (!rule) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Rule not found',
          detail: 'The selected rule does not exist.',
        });
        this.router.navigate(['/payroll/recurring-rules']);
        return;
      }
      this.patchForm(rule);
    });
  }

  patchForm(rule: RecurringRule): void {
    this.form.patchValue({
      name: rule.name,
      type: rule.type,
      status: rule.status,
      priority: rule.priority,
      frequency: rule.frequency,
      amountType: rule.amountType,
      amountValue: rule.amountValue,
      scopeType: rule.scope.type,
      companyId: rule.scope.companyId ?? null,
      branchId: rule.scope.branchId ?? null,
      costCenterId: rule.scope.costCenterId ?? null,
      employeeCategory: rule.scope.employeeCategory ?? null,
      employeeIds: rule.scope.employeeIds ?? [],
      startDate: rule.startDate,
      endDate: rule.endDate ?? null,
    });

    this.loadBranches();
    this.loadCostCenters();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.form.getRawValue() as RecurringRuleFormValue;

    const request$ = this.ruleId
      ? this.recurringRuleService.updateRule(this.ruleId, payload)
      : this.recurringRuleService.createRule(payload);

    request$.subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Rule saved',
        detail: 'Recurring rule has been saved successfully.',
      });
      this.router.navigate(['/payroll/recurring-rules']);
    });
  }

  cancel(): void {
    this.router.navigate(['/payroll/recurring-rules']);
  }

  enableEdit(): void {
    if (this.ruleId) {
      this.router.navigate(['/payroll/recurring-rules', this.ruleId, 'edit']);
    }
  }

  get previewAmount(): number {
    const amountType = this.form.get('amountType')?.value;
    const amountValue = Number(this.form.get('amountValue')?.value || 0);
    if (amountType === 'Percentage') {
      return (this.sampleSalary * amountValue) / 100;
    }
    return amountValue;
  }

  get previewLabel(): string {
    const amountType = this.form.get('amountType')?.value;
    if (amountType === 'Percentage') {
      return `${this.form.get('amountValue')?.value || 0}% of base salary`;
    }
    return 'Fixed amount';
  }

  onCompanyChange(value: string | null): void {
    this.form.patchValue({ companyId: value, branchId: null, costCenterId: null });
    this.loadBranches();
    this.loadCostCenters();
  }

  onBranchChange(value: string | null): void {
    this.form.patchValue({ branchId: value, costCenterId: null });
    this.loadCostCenters();
  }

  private updateAmountValidators(amountType: string): void {
    const amountControl = this.form.get('amountValue');
    if (!amountControl) {
      return;
    }

    if (amountType === 'Percentage') {
      amountControl.setValidators([Validators.required, Validators.min(0.01), Validators.max(100)]);
    } else {
      amountControl.setValidators([Validators.required, Validators.min(0.01)]);
    }
    amountControl.updateValueAndValidity();
  }

  private updateScopeDefaults(): void {
    const scopeType = this.form.get('scopeType')?.value;
    if (scopeType === 'All') {
      this.form.patchValue({
        companyId: null,
        branchId: null,
        costCenterId: null,
        employeeCategory: null,
        employeeIds: [],
      });
    }

    if (scopeType === 'Selected') {
      this.form.patchValue({
        companyId: null,
        branchId: null,
        costCenterId: null,
        employeeCategory: null,
      });
    }
  }

  private dateRangeValidator(group: FormGroup): { invalidRange: boolean } | null {
    const startDate = group.get('startDate')?.value;
    const endDate = group.get('endDate')?.value;
    if (!startDate || !endDate) {
      return null;
    }
    return new Date(endDate) >= new Date(startDate) ? null : { invalidRange: true };
  }

  private todayAsInput(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
