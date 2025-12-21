import { Injectable } from '@angular/core';
import { delay, Observable, of } from 'rxjs';
import {
  FilterOption,
  ReportDefinition,
  ReportFilters,
  ReportKey,
  ReportResult,
  ReportRow,
} from '../models/report.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private cachedReports = new Map<ReportKey, ReportResult>();

  getReportDefinitions(): ReportDefinition[] {
    return [
      {
        key: 'epf-etf',
        title: 'EPF/ETF Contribution Summary',
        description: 'Monthly statutory contributions with employee and employer totals ready for audit review.',
        cadence: 'Monthly',
        groupBy: 'branch',
        groupByLabel: 'Branch',
        columns: [
          { field: 'employee', header: 'Employee', type: 'text' },
          { field: 'nic', header: 'NIC', type: 'text' },
          { field: 'epfNumber', header: 'EPF No.', type: 'text' },
          { field: 'contributableBase', header: 'Contributable Base (LKR)', type: 'currency', align: 'right' },
          { field: 'employeeEpf', header: 'Employee EPF (LKR)', type: 'currency', align: 'right' },
          { field: 'employerEpf', header: 'Employer EPF (LKR)', type: 'currency', align: 'right' },
          { field: 'employerEtf', header: 'Employer ETF (LKR)', type: 'currency', align: 'right' },
        ],
      },
      {
        key: 'paye',
        title: 'PAYE Tax Summary',
        description: 'Monthly PAYE deductions with taxable income breakdown per employee.',
        cadence: 'Monthly',
        groupBy: 'costCenter',
        groupByLabel: 'Cost Center',
        columns: [
          { field: 'employee', header: 'Employee', type: 'text' },
          { field: 'taxableIncome', header: 'Taxable Income (LKR)', type: 'currency', align: 'right' },
          { field: 'paye', header: 'PAYE Tax (LKR)', type: 'currency', align: 'right' },
          { field: 'relief', header: 'Relief Applied (LKR)', type: 'currency', align: 'right' },
          { field: 'taxBand', header: 'Tax Band', type: 'text' },
        ],
      },
      {
        key: 'payroll-register',
        title: 'Payroll Register',
        description: 'Consolidated payroll register by company and branch for monthly reconciliation.',
        cadence: 'Monthly',
        groupBy: 'branch',
        groupByLabel: 'Branch',
        columns: [
          { field: 'employee', header: 'Employee', type: 'text' },
          { field: 'designation', header: 'Designation', type: 'text' },
          { field: 'grossPay', header: 'Gross Pay (LKR)', type: 'currency', align: 'right' },
          { field: 'deductions', header: 'Total Deductions (LKR)', type: 'currency', align: 'right' },
          { field: 'netPay', header: 'Net Pay (LKR)', type: 'currency', align: 'right' },
        ],
      },
      {
        key: 'bank-transfer',
        title: 'Bank Transfer Schedule',
        description: 'Payments grouped by bank for payroll disbursement and upload preparation.',
        cadence: 'Monthly',
        groupBy: 'bank',
        groupByLabel: 'Bank',
        columns: [
          { field: 'employee', header: 'Employee', type: 'text' },
          { field: 'accountNumber', header: 'Account Number', type: 'text' },
          { field: 'bankBranch', header: 'Bank Branch', type: 'text' },
          { field: 'netPay', header: 'Net Pay (LKR)', type: 'currency', align: 'right' },
        ],
      },
      {
        key: 'employer-cost',
        title: 'Employer Cost Summary',
        description: 'Optional view of monthly employer cost including statutory contributions and benefits.',
        cadence: 'Monthly',
        groupBy: 'branch',
        groupByLabel: 'Branch',
        columns: [
          { field: 'employee', header: 'Employee', type: 'text' },
          { field: 'grossPay', header: 'Gross Pay (LKR)', type: 'currency', align: 'right' },
          { field: 'statutoryCost', header: 'Statutory Cost (LKR)', type: 'currency', align: 'right' },
          { field: 'benefits', header: 'Benefits (LKR)', type: 'currency', align: 'right' },
          { field: 'totalCost', header: 'Total Cost (LKR)', type: 'currency', align: 'right' },
        ],
      },
    ];
  }

  getFilterOptions(): {
    companies: FilterOption[];
    branches: FilterOption[];
    costCenters: FilterOption[];
    employeeStatuses: FilterOption[];
    outputModes: FilterOption[];
    outputFormats: FilterOption[];
  } {
    return {
      companies: [
        { label: 'Ceylon Holdings', value: 'company-1' },
        { label: 'Lanka Manufacturing', value: 'company-2' },
      ],
      branches: [
        { label: 'Colombo HQ', value: 'branch-1' },
        { label: 'Kandy Plant', value: 'branch-2' },
        { label: 'Galle Office', value: 'branch-3' },
      ],
      costCenters: [
        { label: 'Finance', value: 'cost-1' },
        { label: 'Operations', value: 'cost-2' },
        { label: 'Sales', value: 'cost-3' },
      ],
      employeeStatuses: [
        { label: 'Active Employees', value: 'active' },
        { label: 'All Employees', value: 'all' },
      ],
      outputModes: [
        { label: 'Preview in app', value: 'preview' },
        { label: 'Export file', value: 'export' },
      ],
      outputFormats: [
        { label: 'CSV', value: 'csv' },
        { label: 'Excel (coming soon)', value: 'excel' },
        { label: 'PDF (print view)', value: 'pdf' },
      ],
    };
  }

  generateReport(reportKey: ReportKey, filters: ReportFilters, generatedBy: string): Observable<ReportResult> {
    const definition = this.getReportDefinition(reportKey);
    const rows = this.getMockRows(reportKey);
    const filtersUsed = this.formatFilters(filters);

    const result: ReportResult = {
      key: reportKey,
      title: definition.title,
      generatedOn: new Date(),
      generatedBy,
      filtersUsed,
      isPeriodClosed: reportKey !== 'payroll-register',
      warnings: reportKey === 'payroll-register' ? ['Payroll period is not closed for the selected month.'] : [],
      groupBy: definition.groupBy,
      groupByLabel: definition.groupByLabel,
      columns: definition.columns,
      rows,
    };

    this.cachedReports.set(reportKey, result);
    return of(result).pipe(delay(700));
  }

  getCachedReport(reportKey: ReportKey): ReportResult | undefined {
    return this.cachedReports.get(reportKey);
  }

  getReportDefinition(reportKey: ReportKey): ReportDefinition {
    const definition = this.getReportDefinitions().find(report => report.key === reportKey);
    if (!definition) {
      return this.getReportDefinitions()[0];
    }
    return definition;
  }

  private formatFilters(filters: ReportFilters): Record<string, string> {
    return {
      Company: this.findLabel(this.getFilterOptions().companies, filters.companyId) || 'All Companies',
      Branch: this.findLabel(this.getFilterOptions().branches, filters.branchId) || 'All Branches',
      'Cost Center': this.findLabel(this.getFilterOptions().costCenters, filters.costCenterId) || 'All Cost Centers',
      'Payroll Month': filters.payrollMonth
        ? filters.payrollMonth.toLocaleDateString('en-US', { month: 'long', year: 'numeric' })
        : 'Current Month',
      'Employee Status': filters.employeeStatus === 'all' ? 'All Employees' : 'Active Employees',
      Output: `${filters.outputMode === 'export' ? 'Export' : 'Preview'} (${filters.outputFormat.toUpperCase()})`,
    };
  }

  private findLabel(options: FilterOption[], value: string | null): string | undefined {
    if (!value) {
      return undefined;
    }
    return options.find(option => option.value === value)?.label;
  }

  private getMockRows(reportKey: ReportKey): ReportRow[] {
    switch (reportKey) {
      case 'epf-etf':
        return [
          {
            branch: 'Colombo HQ',
            employee: 'EMP-001 - S. Perera',
            nic: '901234567V',
            epfNumber: 'EPF-1023',
            contributableBase: 125000,
            employeeEpf: 10000,
            employerEpf: 12000,
            employerEtf: 3750,
          },
          {
            branch: 'Colombo HQ',
            employee: 'EMP-014 - I. Jayasinghe',
            nic: '881112223V',
            epfNumber: 'EPF-1188',
            contributableBase: 98000,
            employeeEpf: 7840,
            employerEpf: 9408,
            employerEtf: 2940,
          },
          {
            branch: 'Kandy Plant',
            employee: 'EMP-121 - D. Fernando',
            nic: '921234561V',
            epfNumber: 'EPF-1342',
            contributableBase: 87500,
            employeeEpf: 7000,
            employerEpf: 8400,
            employerEtf: 2625,
          },
        ];
      case 'paye':
        return [
          {
            costCenter: 'Finance',
            employee: 'EMP-006 - K. Silva',
            taxableIncome: 210000,
            paye: 22500,
            relief: 15000,
            taxBand: '18%',
          },
          {
            costCenter: 'Finance',
            employee: 'EMP-009 - R. Abeysekera',
            taxableIncome: 145000,
            paye: 10250,
            relief: 12500,
            taxBand: '12%',
          },
          {
            costCenter: 'Operations',
            employee: 'EMP-031 - T. Ranasinghe',
            taxableIncome: 98000,
            paye: 4900,
            relief: 10000,
            taxBand: '6%',
          },
        ];
      case 'payroll-register':
        return [
          {
            branch: 'Colombo HQ',
            employee: 'EMP-002 - N. Weerasinghe',
            designation: 'Accountant',
            grossPay: 180000,
            deductions: 32000,
            netPay: 148000,
          },
          {
            branch: 'Colombo HQ',
            employee: 'EMP-017 - J. Liyanage',
            designation: 'HR Executive',
            grossPay: 132000,
            deductions: 21000,
            netPay: 111000,
          },
          {
            branch: 'Galle Office',
            employee: 'EMP-088 - P. Wickramasinghe',
            designation: 'Sales Lead',
            grossPay: 165000,
            deductions: 28500,
            netPay: 136500,
          },
        ];
      case 'bank-transfer':
        return [
          {
            bank: 'Bank of Ceylon',
            employee: 'EMP-010 - M. Dias',
            accountNumber: '100-456-789',
            bankBranch: 'Lake House',
            netPay: 92000,
          },
          {
            bank: 'Bank of Ceylon',
            employee: 'EMP-042 - S. Peris',
            accountNumber: '100-456-112',
            bankBranch: 'Kollupitiya',
            netPay: 105500,
          },
          {
            bank: 'Commercial Bank',
            employee: 'EMP-053 - A. Kariyawasam',
            accountNumber: '200-210-884',
            bankBranch: 'Kandy City',
            netPay: 87000,
          },
        ];
      case 'employer-cost':
      default:
        return [
          {
            branch: 'Colombo HQ',
            employee: 'EMP-023 - D. Samarasinghe',
            grossPay: 200000,
            statutoryCost: 27500,
            benefits: 12000,
            totalCost: 239500,
          },
          {
            branch: 'Kandy Plant',
            employee: 'EMP-077 - P. Wijesinghe',
            grossPay: 155000,
            statutoryCost: 21000,
            benefits: 8500,
            totalCost: 184500,
          },
          {
            branch: 'Kandy Plant',
            employee: 'EMP-081 - G. Herath',
            grossPay: 142000,
            statutoryCost: 19300,
            benefits: 7800,
            totalCost: 169100,
          },
        ];
    }
  }
}
