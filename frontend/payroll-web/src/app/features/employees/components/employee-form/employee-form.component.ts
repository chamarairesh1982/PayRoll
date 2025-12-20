import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { Employee } from '../../models/employee.model';

@Component({
  selector: 'app-employee-form',
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss'],
})
export class EmployeeFormComponent implements OnInit, OnChanges {
  @Input() initialValue: Partial<Employee> | null | undefined;
  @Input() mode: 'create' | 'edit' = 'create';
  @Output() submitted = new EventEmitter<Partial<Employee>>();

  form: FormGroup;

  genders: Employee['gender'][] = ['Male', 'Female', 'Other'];
  maritalStatuses: Employee['maritalStatus'][] = ['Single', 'Married', 'Other'];
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];

  constructor(
    private fb: FormBuilder,
    private organizationApi: OrganizationApiService,
  ) {
    this.form = this.fb.group({
      employeeCode: ['', Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      initials: [''],
      callingName: [''],
      nicNumber: ['', [Validators.required, Validators.minLength(10)]],
      epfNumber: [''],
      dateOfBirth: ['', Validators.required],
      gender: ['Male', Validators.required],
      maritalStatus: ['Single', Validators.required],
      employmentStartDate: ['', Validators.required],
      probationEndDate: [''],
      confirmationDate: [''],
      baseSalary: [0, [Validators.required, Validators.min(0.01)]],
      hourlyRate: [null],
      bankName: [''],
      bankCode: [''],
      branchCode: [''],
      bankAccountNumber: [''],
      companyId: [''],
      branchId: [''],
      costCenterId: [''],
      isActive: [true],
    });
  }

  ngOnInit(): void {
    this.loadCompanies();

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
    if (changes['initialValue'] && this.initialValue) {
      const normalizedValue: Partial<Employee> = {
        ...this.initialValue,
        dateOfBirth: this.normalizeDateInput(this.initialValue.dateOfBirth),
        employmentStartDate: this.normalizeDateInput(this.initialValue.employmentStartDate),
        probationEndDate: this.normalizeDateInput(this.initialValue.probationEndDate),
        confirmationDate: this.normalizeDateInput(this.initialValue.confirmationDate),
      };
      this.form.patchValue(normalizedValue, { emitEvent: false });
      const companyId = this.form.get('companyId')?.value || undefined;
      const branchId = this.form.get('branchId')?.value || undefined;
      this.loadBranches(companyId);
      this.loadCostCenters(companyId, branchId);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.value;
    const payload: Partial<Employee> = {
      ...value,
      initials: value.initials || null,
      callingName: value.callingName || null,
      epfNumber: value.epfNumber || null,
      probationEndDate: value.probationEndDate || null,
      confirmationDate: value.confirmationDate || null,
      hourlyRate: value.hourlyRate ?? null,
      bankName: value.bankName || null,
      bankCode: value.bankCode || null,
      branchCode: value.branchCode || null,
      bankAccountNumber: value.bankAccountNumber || null,
      companyId: value.companyId || null,
      branchId: value.branchId || null,
      costCenterId: value.costCenterId || null,
    };
    this.submitted.emit(payload);
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

  private normalizeDateInput(value?: string | null): string {
    if (!value) {
      return '';
    }

    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return value;
    }

    const datePart = value.split('T')[0]?.split(' ')[0];
    return datePart || '';
  }
}
