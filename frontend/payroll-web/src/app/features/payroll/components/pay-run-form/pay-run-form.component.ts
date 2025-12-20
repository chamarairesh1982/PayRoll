import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Employee } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { PayPeriodType, PayRunSummary } from '../../models/pay-run.model';

@Component({
  selector: 'app-pay-run-form',
  templateUrl: './pay-run-form.component.html',
  styleUrls: ['./pay-run-form.component.scss'],
})
export class PayRunFormComponent implements OnInit, OnChanges {
  @Input() initialValue?: Partial<PayRunSummary> | null;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<{
    name: string;
    periodType: PayPeriodType;
    periodStart: string;
    periodEnd: string;
    payDate: string;
    companyId?: string;
    branchId?: string;
    costCenterId?: string;
    isConsolidated?: boolean;
    includeActiveEmployeesOnly: boolean;
    employeeIds?: string[];
  }>();

  form: FormGroup;
  periodTypes: PayPeriodType[] = ['Monthly', 'Weekly', 'Custom'];

  employees: Employee[] = [];
  filteredEmployees: Employee[] = [];
  loadingEmployees = true;
  employeesLoadError: string | null = null;
  employeeSearch = '';
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];

  constructor(
    private fb: FormBuilder,
    private employeesApi: EmployeesApiService,
    private organizationApi: OrganizationApiService,
  ) {
    this.form = this.fb.group(
      {
        name: ['', Validators.required],
        periodType: ['Monthly', Validators.required],
        periodStart: ['', Validators.required],
        periodEnd: ['', Validators.required],
        payDate: ['', Validators.required],
        companyId: [''],
        branchId: [''],
        costCenterId: [''],
        isConsolidated: [false],
        includeActiveEmployeesOnly: [true],
        employeeIds: [[]],
      },
      { validators: [this.periodRangeValidator, this.scopeValidator] },
    );
  }

  ngOnInit(): void {
    if (this.initialValue) {
      this.form.patchValue(this.initialValue);
    }

    this.loadCompanies();
    this.loadEmployees();

    this.form.get('includeActiveEmployeesOnly')?.valueChanges.subscribe(() => {
      this.applyFilters();
    });

    this.form.get('companyId')?.valueChanges.subscribe(companyId => {
      this.form.patchValue({ branchId: '', costCenterId: '' }, { emitEvent: false });
      this.loadBranches(companyId || undefined);
      this.loadCostCenters(companyId || undefined, undefined);
    });

    this.form.get('branchId')?.valueChanges.subscribe(branchId => {
      const companyId = this.form.get('companyId')?.value || undefined;
      this.form.patchValue({ costCenterId: '' }, { emitEvent: false });
      this.loadCostCenters(companyId, branchId || undefined);
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialValue'] && changes['initialValue'].currentValue) {
      this.form.patchValue(changes['initialValue'].currentValue);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.value as {
      name: string;
      periodType: PayPeriodType;
      periodStart: string;
      periodEnd: string;
      payDate: string;
      companyId?: string;
      branchId?: string;
      costCenterId?: string;
      isConsolidated?: boolean;
      includeActiveEmployeesOnly: boolean;
      employeeIds?: string[];
    };

    const employeeIds = value.employeeIds || [];

    this.submitted.emit({
      name: value.name,
      periodType: value.periodType,
      periodStart: value.periodStart,
      periodEnd: value.periodEnd,
      payDate: value.payDate,
      ...(value.companyId ? { companyId: value.companyId } : {}),
      ...(value.branchId ? { branchId: value.branchId } : {}),
      ...(value.costCenterId ? { costCenterId: value.costCenterId } : {}),
      ...(value.isConsolidated !== undefined ? { isConsolidated: value.isConsolidated } : {}),
      includeActiveEmployeesOnly: value.includeActiveEmployeesOnly,
      ...(employeeIds && employeeIds.length ? { employeeIds } : {}),
    });
  }

  onEmployeeSearchChange(term: string): void {
    this.employeeSearch = term;
    this.applyFilters();
  }

  private loadEmployees(): void {
    this.loadingEmployees = true;
    this.employeesLoadError = null;
    this.employeesApi.getEmployees(1, 1000).subscribe({
      next: result => {
        this.employees = result.items;
        this.loadingEmployees = false;
        this.applyFilters();
      },
      error: err => {
        console.error('Failed to load employees', err);
        this.employeesLoadError = 'Failed to load employees. Please try again later.';
        this.loadingEmployees = false;
        this.filteredEmployees = [];
      },
    });
  }

  private loadCompanies(): void {
    this.organizationApi.getCompanies().subscribe(companies => {
      this.companies = companies;
      const companyId = this.form.get('companyId')?.value || undefined;
      this.loadBranches(companyId);
      const branchId = this.form.get('branchId')?.value || undefined;
      this.loadCostCenters(companyId, branchId);
    });
  }

  private loadBranches(companyId?: string): void {
    this.organizationApi.getBranches(companyId).subscribe(branches => {
      this.branches = branches;
    });
  }

  private loadCostCenters(companyId?: string, branchId?: string): void {
    this.organizationApi.getCostCenters(companyId, branchId).subscribe(costCenters => {
      this.costCenters = costCenters;
    });
  }

  private applyFilters(): void {
    const includeActiveOnly = this.form.get('includeActiveEmployeesOnly')?.value;
    const search = this.employeeSearch.trim().toLowerCase();

    let results = this.employees;

    if (includeActiveOnly) {
      results = results.filter(emp => emp.isActive);
    }

    if (search) {
      results = results.filter(emp => {
        const fields = [
          emp.firstName,
          emp.lastName,
          emp.employeeCode,
          emp.callingName ?? '',
          emp.initials ?? '',
        ]
          .filter(Boolean)
          .map(val => val.toLowerCase());

        return fields.some(field => field.includes(search));
      });
    }

    this.filteredEmployees = results;
  }

  private periodRangeValidator = (group: FormGroup) => {
    const start = group.get('periodStart')?.value;
    const end = group.get('periodEnd')?.value;

    if (start && end && new Date(end) < new Date(start)) {
      return { periodRange: true };
    }

    return null;
  };

  private scopeValidator = (group: FormGroup) => {
    const isConsolidated = group.get('isConsolidated')?.value;
    const companyId = group.get('companyId')?.value;
    const branchId = group.get('branchId')?.value;
    const costCenterId = group.get('costCenterId')?.value;

    if (!isConsolidated && !companyId && !branchId && !costCenterId) {
      return { scopeRequired: true };
    }

    return null;
  };

  get periodRangeInvalid(): boolean {
    return !!this.form.errors?.['periodRange'] && this.form.get('periodEnd')?.touched;
  }

  get scopeInvalid(): boolean {
    if (!this.form.errors?.['scopeRequired']) {
      return false;
    }

    const companyTouched = this.form.get('companyId')?.touched;
    const branchTouched = this.form.get('branchId')?.touched;
    const costCenterTouched = this.form.get('costCenterId')?.touched;

    return !!(companyTouched || branchTouched || costCenterTouched);
  }
}
