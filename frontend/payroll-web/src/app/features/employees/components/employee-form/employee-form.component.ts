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
      nicNumber: ['', [Validators.required, Validators.minLength(10)]],
      dateOfBirth: ['', Validators.required],
      gender: ['Male', Validators.required],
      maritalStatus: ['Single', Validators.required],
      employmentStartDate: ['', Validators.required],
      probationEndDate: [''],
      confirmationDate: [''],
      baseSalary: [0, [Validators.required, Validators.min(0.01)]],
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
      this.form.patchValue(this.initialValue);
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
    this.submitted.emit(this.form.value);
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
}
