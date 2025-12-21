import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { Employee } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { EmployeeTaxProfile, TaxExemptionKey } from '../../models/tax-profile.model';
import { TaxProfileService } from '../../services/tax-profile.service';

interface EmployeeTaxRow {
  employee: Employee;
  profile: EmployeeTaxProfile | null;
}

@Component({
  selector: 'app-tax-profiles-page',
  templateUrl: './tax-profiles-page.component.html',
  styleUrls: ['./tax-profiles-page.component.scss'],
  providers: [MessageService],
})
export class TaxProfilesPageComponent implements OnInit, OnDestroy {
  rows: EmployeeTaxRow[] = [];
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
  ) {}

  ngOnInit(): void {
    combineLatest([
      this.employeesApi.getEmployees(1, 200, {}),
      this.taxProfileService.getProfiles(),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([employeeResult, profiles]) => {
        this.rows = employeeResult.items.map(employee => ({
          employee,
          profile: profiles.find(profile => profile.employeeId === employee.id) ?? null,
        }));
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
