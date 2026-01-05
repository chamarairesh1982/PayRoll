import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { Employee } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { EmployeeTaxProfile, TaxExemptionKey } from '../../models/tax-profile.model';
import { TaxProfileService } from '../../services/tax-profile.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

interface EmployeeTaxRow {
  employee: Employee;
  profile: EmployeeTaxProfile | null;
  // Computed fields for flat table access
  employeeName?: string;
  taxCategory?: string;
  isResidentLabel?: string;
}

@Component({
  selector: 'app-tax-profiles-page',
  templateUrl: './tax-profiles-page.component.html',
  styleUrls: ['./tax-profiles-page.component.scss'],
  providers: [MessageService],
})
export class TaxProfilesPageComponent implements OnInit, OnDestroy {
  rows: EmployeeTaxRow[] = [];
  isLoading = false;

  columns: DataTableColumn<EmployeeTaxRow>[] = [
    { field: 'employeeName', header: 'Taxpayer Identity', sortable: true, minWidth: '250px' },
    { field: 'taxCategory', header: 'Category', sortable: true, type: 'badge', minWidth: '150px' },
    { field: 'isResidentLabel', header: 'Residency', sortable: true, type: 'status', minWidth: '150px' },
    { field: 'profile.exemptions', header: 'Reliefs & Exemptions', minWidth: '250px' },
  ];
  dialogVisible = false;
  activeRow: EmployeeTaxRow | null = null;
  form: FormGroup = this.fb.group({
    taxCategory: ['', Validators.required],
    isResident: [true],
    PrimaryEmployment: [false],
    SeniorCitizen: [false],
    Disabled: [false],
    ForeignIncome: [false],
  });

  taxCategories = this.taxProfileService.getTaxCategories();

  exemptionOptions: { key: TaxExemptionKey; label: string }[] = [
    { key: 'PrimaryEmployment', label: 'Primary employment relief' },
    { key: 'SeniorCitizen', label: 'Senior citizen exemption' },
    { key: 'Disabled', label: 'Disability exemption' },
    { key: 'ForeignIncome', label: 'Foreign income excluded' },
  ];

  private destroy$ = new Subject<void>();

  constructor(
    private employeesApi: EmployeesApiService,
    private taxProfileService: TaxProfileService,
    private fb: FormBuilder,
    private messageService: MessageService,
  ) { }

  ngOnInit(): void {
    this.isLoading = true;
    combineLatest([
      this.employeesApi.getEmployees(1, 1000, {}),
      this.taxProfileService.getProfiles(),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ([employeeResult, profiles]) => {
          this.rows = employeeResult.items.map(employee => {
            const profile = profiles.find(p => p.employeeId === employee.id) ?? null;
            return {
              employee,
              profile,
              employeeName: `${employee.firstName} ${employee.lastName}`,
              taxCategory: profile?.taxCategory || 'Pending adjudication',
              isResidentLabel: profile ? (profile.isResident ? 'Resident' : 'Non-resident') : 'Undefined',
            };
          });
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openEditor(row: EmployeeTaxRow): void {
    this.activeRow = row;
    const profile = row.profile ?? this.taxProfileService.createProfile(row.employee.id);

    this.form.reset({
      taxCategory: profile.taxCategory,
      isResident: profile.isResident,
      PrimaryEmployment: profile.exemptions.PrimaryEmployment,
      SeniorCitizen: profile.exemptions.SeniorCitizen,
      Disabled: profile.exemptions.Disabled,
      ForeignIncome: profile.exemptions.ForeignIncome,
    });

    this.dialogVisible = true;
  }

  hasAnyExemption(profile: EmployeeTaxProfile): boolean {
    return Object.values(profile.exemptions).some(val => !!val);
  }

  saveProfile(): void {
    if (!this.activeRow) {
      return;
    }

    this.form.markAllAsTouched();
    if (this.form.invalid) {
      return;
    }

    const existing = this.activeRow.profile ?? this.taxProfileService.createProfile(this.activeRow.employee.id);
    const payload: EmployeeTaxProfile = {
      ...existing,
      taxCategory: this.form.value.taxCategory,
      isResident: this.form.value.isResident,
      exemptions: {
        PrimaryEmployment: this.form.value.PrimaryEmployment,
        SeniorCitizen: this.form.value.SeniorCitizen,
        Disabled: this.form.value.Disabled,
        ForeignIncome: this.form.value.ForeignIncome,
      },
      lastUpdated: new Date().toISOString(),
    };

    this.taxProfileService.saveProfile(payload);
    this.messageService.add({
      severity: 'success',
      summary: 'Tax profile saved',
      detail: `Tax profile updated for ${this.activeRow.employee.firstName} ${this.activeRow.employee.lastName}.`,
    });

    this.dialogVisible = false;
  }
}
