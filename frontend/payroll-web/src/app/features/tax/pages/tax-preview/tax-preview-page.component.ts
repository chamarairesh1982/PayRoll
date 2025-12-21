import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Subject, combineLatest, takeUntil } from 'rxjs';
import { Employee } from '../../../employees/models/employee.model';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { TaxPreviewRow } from '../../models/tax-preview.model';
import { EmployeeTaxProfile } from '../../models/tax-profile.model';
import { TaxScheme } from '../../models/tax-scheme.model';
import { TaxCalculationService } from '../../services/tax-calculation.service';
import { TaxConfigService } from '../../services/tax-config.service';
import { TaxProfileService } from '../../services/tax-profile.service';

@Component({
  selector: 'app-tax-preview-page',
  templateUrl: './tax-preview-page.component.html',
  styleUrls: ['./tax-preview-page.component.scss'],
})
export class TaxPreviewPageComponent implements OnInit, OnDestroy {
  form: FormGroup = this.fb.group({
    month: [new Date()],
    scopeType: ['All'],
    employeeId: [''],
  });

  employees: Employee[] = [];
  employeeOptions: { label: string; value: string }[] = [];
  profiles: EmployeeTaxProfile[] = [];
  schemes: TaxScheme[] = [];
  activeScheme: TaxScheme | null = null;
  previewRows: TaxPreviewRow[] = [];

  totals = {
    grossTaxable: 0,
    reliefTotal: 0,
    taxableIncome: 0,
    calculatedTax: 0,
  };

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private employeesApi: EmployeesApiService,
    private taxProfileService: TaxProfileService,
    private taxConfigService: TaxConfigService,
    private taxCalculationService: TaxCalculationService,
  ) {}

  ngOnInit(): void {
    combineLatest([
      this.employeesApi.getEmployees(1, 200, {}),
      this.taxProfileService.getProfiles(),
      this.taxConfigService.getSchemes(),
    ])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([employeesResult, profiles, schemes]) => {
        this.employees = employeesResult.items;
        this.employeeOptions = this.employees.map(employee => ({
          label: `${employee.firstName} ${employee.lastName} (${employee.employeeCode})`,
          value: employee.id,
        }));
        this.profiles = profiles;
        this.schemes = schemes;
        this.updateActiveScheme();
        this.runPreview();
      });

    this.form.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.updateActiveScheme();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  runPreview(): void {
    if (!this.activeScheme) {
      this.previewRows = [];
      this.totals = { grossTaxable: 0, reliefTotal: 0, taxableIncome: 0, calculatedTax: 0 };
      return;
    }

    const scopeType = this.form.value.scopeType;
    const selectedEmployeeId = this.form.value.employeeId;

    const selectedEmployees =
      scopeType === 'Employee'
        ? selectedEmployeeId
          ? this.employees.filter(employee => employee.id === selectedEmployeeId)
          : []
        : this.employees;

    this.previewRows = selectedEmployees.map(employee => {
      const profile = this.profiles.find(item => item.employeeId === employee.id) ?? null;
      const grossTaxable = employee.baseSalary ?? 0;
      const calculation = this.taxCalculationService.calculateMonthlyTax(
        grossTaxable,
        this.activeScheme as TaxScheme,
        this.activeScheme?.reliefs ?? [],
      );

      return {
        employeeId: employee.id,
        employeeName: `${employee.firstName} ${employee.lastName}`,
        taxCategory: profile?.taxCategory ?? 'Unassigned',
        ...calculation,
      };
    });

    this.totals = this.previewRows.reduce(
      (totals, row) => ({
        grossTaxable: totals.grossTaxable + row.grossTaxable,
        reliefTotal: totals.reliefTotal + row.reliefTotal,
        taxableIncome: totals.taxableIncome + row.taxableIncome,
        calculatedTax: totals.calculatedTax + row.calculatedTax,
      }),
      { grossTaxable: 0, reliefTotal: 0, taxableIncome: 0, calculatedTax: 0 },
    );
  }

  updateActiveScheme(): void {
    const month = this.form.value.month as Date;
    if (!month) {
      this.activeScheme = null;
      return;
    }

    const isoMonth = new Date(month);
    const monthKey = isoMonth.toISOString().split('T')[0];
    this.activeScheme =
      this.schemes.find(scheme => {
        const end = scheme.effectiveTo ?? '9999-12-31';
        return scheme.isActive && monthKey >= scheme.effectiveFrom && monthKey <= end;
      }) ?? null;
  }
}
