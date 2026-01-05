import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { BankBranch } from '../../../payroll-config/models/bank-branch.model';
import { Bank } from '../../../payroll-config/models/bank.model';
import { BankBranchesApiService } from '../../../payroll-config/services/bank-branches-api.service';
import { BanksApiService } from '../../../payroll-config/services/banks-api.service';
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
  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;

  genders: Employee['gender'][] = ['Male', 'Female', 'Other'];
  maritalStatuses: Employee['maritalStatus'][] = ['Single', 'Married', 'Other'];
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];
  banks: Bank[] = [];
  bankBranches: BankBranch[] = [];

  constructor(
    private fb: FormBuilder,
    private organizationApi: OrganizationApiService,
    private banksApi: BanksApiService,
    private bankBranchesApi: BankBranchesApiService,
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
    this.loadBanks();

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

    this.form.get('bankCode')?.valueChanges.subscribe(bankCode => {
      const selectedBank = this.banks.find(bank => bank.code === bankCode);

      if (selectedBank) {
        this.form.patchValue(
          { bankName: selectedBank.name, branchCode: '' },
          { emitEvent: false },
        );
        this.loadBankBranches(selectedBank.id);
      } else if (bankCode) {
        this.ensureLegacyBankOption(bankCode);
        this.bankBranches = this.buildLegacyBranches();
      } else {
        this.form.patchValue({ bankName: '', branchCode: '' }, { emitEvent: false });
        this.bankBranches = [];
      }

      this.updateBankValidators();
    });

    this.form.get('branchCode')?.valueChanges.subscribe(() => {
      this.updateBankValidators();
    });

    this.form.get('bankAccountNumber')?.valueChanges.subscribe(() => {
      this.updateBankValidators();
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
      const companyId = normalizedValue.companyId ?? undefined;
      const branchId = normalizedValue.branchId ?? undefined;
      const costCenterId = normalizedValue.costCenterId ?? undefined;
      this.loadBranches(companyId, branchId);
      this.loadCostCenters(companyId, branchId, costCenterId);

      if (normalizedValue.bankCode) {
        const selectedBank = this.banks.find(bank => bank.code === normalizedValue.bankCode);
        if (selectedBank) {
          this.loadBankBranches(selectedBank.id, normalizedValue.branchCode || undefined);
          this.form.patchValue({ bankName: selectedBank.name }, { emitEvent: false });
        } else {
          this.ensureLegacyBankOption(normalizedValue.bankCode);
          this.bankBranches = this.buildLegacyBranches();
        }
      }

      this.updateBankValidators();
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

  cancel(): void {
    this.cancelled.emit();
  }

  isInvalid(controlName: string): boolean {
    const control = this.form.get(controlName);
    return !!control && control.invalid && (control.dirty || control.touched);
  }

  private loadCompanies(): void {
    this.organizationApi.getCompanies().subscribe(companies => {
      this.companies = companies;
      const companyId = this.form.get('companyId')?.value || undefined;
      const branchId = this.form.get('branchId')?.value || undefined;
      const costCenterId = this.form.get('costCenterId')?.value || undefined;
      this.loadBranches(companyId, branchId);
      this.loadCostCenters(companyId, branchId, costCenterId);
    });
  }

  private loadBranches(companyId?: string, selectedBranchId?: string): void {
    this.organizationApi.getBranches(companyId).subscribe(branches => {
      this.branches = branches;
      if (selectedBranchId) {
        const hasSelection = branches.some(branch => branch.id === selectedBranchId);
        this.form.patchValue({ branchId: hasSelection ? selectedBranchId : '' }, { emitEvent: false });
      }
    });
  }

  private loadCostCenters(companyId?: string, branchId?: string, selectedCostCenterId?: string): void {
    this.organizationApi.getCostCenters(companyId, branchId).subscribe(costCenters => {
      this.costCenters = costCenters;
      if (selectedCostCenterId) {
        const hasSelection = costCenters.some(costCenter => costCenter.id === selectedCostCenterId);
        this.form.patchValue({ costCenterId: hasSelection ? selectedCostCenterId : '' }, { emitEvent: false });
      }
    });
  }

  private loadBanks(): void {
    this.banksApi.getBanks({ page: 1, pageSize: 200, isActive: null }).subscribe(result => {
      this.banks = result.items;
      const bankCode = this.form.get('bankCode')?.value;
      if (bankCode) {
        const selectedBank = this.banks.find(bank => bank.code === bankCode);
        if (selectedBank) {
          this.loadBankBranches(selectedBank.id, this.form.get('branchCode')?.value || undefined);
          this.form.patchValue({ bankName: selectedBank.name }, { emitEvent: false });
        } else {
          this.ensureLegacyBankOption(bankCode);
          this.bankBranches = this.buildLegacyBranches();
        }
      }
    });
  }

  private loadBankBranches(bankId: string, selectedBranchCode?: string): void {
    this.bankBranchesApi
      .getBankBranches({ page: 1, pageSize: 200, bankId, isActive: true })
      .subscribe(result => {
        this.bankBranches = result.items;
        if (selectedBranchCode) {
          const hasSelection = this.bankBranches.some(branch => branch.code === selectedBranchCode);
          this.form.patchValue({ branchCode: hasSelection ? selectedBranchCode : '' }, { emitEvent: false });
        }
      });
  }

  private ensureLegacyBankOption(bankCode: string): void {
    if (this.banks.some(bank => bank.code === bankCode)) {
      return;
    }

    const legacyName = this.form.get('bankName')?.value || bankCode;
    this.banks = [{ id: 'legacy', code: bankCode, name: legacyName, isActive: true }, ...this.banks];
    this.form.patchValue({ bankName: legacyName }, { emitEvent: false });
  }

  private buildLegacyBranches(): BankBranch[] {
    const branchCode = this.form.get('branchCode')?.value;
    const bankCode = this.form.get('bankCode')?.value;
    const bankName = this.form.get('bankName')?.value || bankCode;
    if (!branchCode || !bankCode) {
      return [];
    }

    return [
      {
        id: 'legacy',
        bankId: 'legacy',
        bankCode,
        bankName: bankName || bankCode,
        code: branchCode,
        name: branchCode,
        isActive: true,
      },
    ];
  }

  private updateBankValidators(): void {
    const bankCodeControl = this.form.get('bankCode');
    const branchCodeControl = this.form.get('branchCode');
    const accountControl = this.form.get('bankAccountNumber');

    const bankCode = bankCodeControl?.value;
    const branchCode = branchCodeControl?.value;
    const account = accountControl?.value;

    if (bankCode) {
      accountControl?.setValidators([Validators.required]);
    } else {
      accountControl?.clearValidators();
    }

    if (account) {
      bankCodeControl?.setValidators([Validators.required]);
      branchCodeControl?.setValidators([Validators.required]);
    } else {
      bankCodeControl?.clearValidators();
      branchCodeControl?.clearValidators();
    }

    accountControl?.updateValueAndValidity({ emitEvent: false });
    bankCodeControl?.updateValueAndValidity({ emitEvent: false });
    branchCodeControl?.updateValueAndValidity({ emitEvent: false });
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
