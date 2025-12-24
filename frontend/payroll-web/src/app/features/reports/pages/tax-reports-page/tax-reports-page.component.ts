import { Component, OnInit } from '@angular/core';
import { EmployeesApiService } from '../../../employees/services/employees-api.service';
import { Employee } from '../../../employees/models/employee.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { TaxDocumentsApiService } from '../../services/tax-documents-api.service';
import {
  AnnualTaxReportRequest,
  MonthlyTaxReportRequest,
  TaxCertificateRequest,
  TaxDocumentHistory,
  TaxDocumentMetadata,
} from '../../models/tax-document.model';
import { FileExportResult } from '../../models/statutory-report.model';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

@Component({
  selector: 'app-tax-reports-page',
  templateUrl: './tax-reports-page.component.html',
  styleUrls: ['./tax-reports-page.component.scss'],
})
export class TaxReportsPageComponent implements OnInit {
  historyColumns: DataTableColumn<TaxDocumentHistory>[] = [
    { field: 'generatedAtUtc', header: 'Archive Date', type: 'datetime', sortable: true, minWidth: '180px' },
    { field: 'type', header: 'Document Specification', sortable: true, minWidth: '150px' },
    { field: 'periodLabel', header: 'Fiscal Interval', sortable: true, minWidth: '150px' },
    { field: 'employeeName', header: 'Personnel Identity', sortable: true, minWidth: '180px' },
    { field: 'status', header: 'Governance Status', sortable: true, type: 'status', minWidth: '150px' },
  ];
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];
  employees: Employee[] = [];
  filteredEmployees: Employee[] = [];
  employeeSearch = '';

  companyFilter = '';
  branchFilter = '';
  costCenterFilter = '';

  monthlyYear = new Date().getFullYear();
  monthlyMonth = new Date().getMonth() + 1;
  monthlyFormat: 'csv' | 'pdf' = 'csv';
  monthlyRegenerate = false;

  annualYear = new Date().getFullYear();
  annualFormat: 'csv' | 'pdf' = 'csv';
  annualRegenerate = false;

  certificateYear = new Date().getFullYear();
  selectedEmployeeId = '';
  certificateRegenerate = false;

  isLoading = false;
  errorMessage: string | null = null;

  monthlyResult: TaxDocumentMetadata | null = null;
  annualResult: TaxDocumentMetadata | null = null;
  certificateResult: TaxDocumentMetadata | null = null;
  history: TaxDocumentHistory[] = [];

  constructor(
    private organizationApi: OrganizationApiService,
    private employeesApi: EmployeesApiService,
    private taxDocumentsApi: TaxDocumentsApiService,
  ) { }

  ngOnInit(): void {
    this.loadOrganizations();
    this.loadEmployees();
    this.loadHistory();
  }

  loadOrganizations(): void {
    this.organizationApi.getCompanies().subscribe({
      next: companies => {
        this.companies = companies;
      },
      error: err => console.error('Failed to load companies', err),
    });
  }

  loadBranches(companyId?: string): void {
    this.organizationApi.getBranches(companyId).subscribe({
      next: branches => {
        this.branches = branches;
      },
      error: err => console.error('Failed to load branches', err),
    });
  }

  loadCostCenters(companyId?: string, branchId?: string): void {
    this.organizationApi.getCostCenters(companyId, branchId).subscribe({
      next: costCenters => {
        this.costCenters = costCenters;
      },
      error: err => console.error('Failed to load cost centers', err),
    });
  }

  loadEmployees(): void {
    this.employeesApi.getEmployees(1, 200).subscribe({
      next: result => {
        this.employees = result.items;
        this.filteredEmployees = result.items;
      },
      error: err => console.error('Failed to load employees', err),
    });
  }

  loadHistory(): void {
    this.taxDocumentsApi.getDocuments().subscribe({
      next: history => {
        this.history = history.map(item => {
          let periodLabel = `${item.year || ''}`;
          if (item.periodStart && item.periodEnd) {
            const start = new Date(item.periodStart);
            const end = new Date(item.periodEnd);
            const isSameMonth = start.getMonth() === end.getMonth() && start.getFullYear() === end.getFullYear();
            periodLabel = isSameMonth
              ? `${start.toLocaleString('default', { month: 'short' })} ${start.getFullYear()}`
              : `${start.toLocaleDateString()} - ${end.toLocaleDateString()}`;
          }
          return { ...item, periodLabel };
        });
      },
      error: err => console.error('Failed to load tax document history', err),
    });
  }

  onCompanyChange(companyId: string): void {
    this.companyFilter = companyId;
    this.branchFilter = '';
    this.costCenterFilter = '';
    this.loadBranches(companyId || undefined);
    this.loadCostCenters(companyId || undefined, undefined);
  }

  onBranchChange(branchId: string): void {
    this.branchFilter = branchId;
    this.costCenterFilter = '';
    this.loadCostCenters(this.companyFilter || undefined, branchId || undefined);
  }

  onEmployeeSearch(query: string): void {
    this.employeeSearch = query;
    const normalized = query.toLowerCase();
    this.filteredEmployees = this.employees.filter(employee =>
      `${employee.employeeCode ?? ''} ${employee.firstName ?? ''} ${employee.lastName ?? ''}`
        .toLowerCase()
        .includes(normalized),
    );
  }

  generateMonthly(): void {
    this.errorMessage = null;
    this.isLoading = true;

    const request: MonthlyTaxReportRequest = {
      year: this.monthlyYear,
      month: this.monthlyMonth,
      format: this.monthlyFormat,
      companyId: this.companyFilter || undefined,
      branchId: this.branchFilter || undefined,
      costCenterId: this.costCenterFilter || undefined,
    };

    this.taxDocumentsApi.generateMonthlyReport(request, this.monthlyRegenerate).subscribe({
      next: document => {
        this.monthlyResult = document;
        this.isLoading = false;
        this.loadHistory();
      },
      error: err => {
        this.errorMessage = err.error?.message || 'Unable to generate monthly APIT report.';
        this.isLoading = false;
      },
    });
  }

  generateAnnual(): void {
    this.errorMessage = null;
    this.isLoading = true;

    const request: AnnualTaxReportRequest = {
      year: this.annualYear,
      format: this.annualFormat,
      companyId: this.companyFilter || undefined,
      branchId: this.branchFilter || undefined,
      costCenterId: this.costCenterFilter || undefined,
    };

    this.taxDocumentsApi.generateAnnualReport(request, this.annualRegenerate).subscribe({
      next: document => {
        this.annualResult = document;
        this.isLoading = false;
        this.loadHistory();
      },
      error: err => {
        this.errorMessage = err.error?.message || 'Unable to generate annual APIT report.';
        this.isLoading = false;
      },
    });
  }

  generateCertificate(): void {
    if (!this.selectedEmployeeId) {
      this.errorMessage = 'Select an employee for the certificate.';
      return;
    }

    this.errorMessage = null;
    this.isLoading = true;

    const request: TaxCertificateRequest = {
      employeeId: this.selectedEmployeeId,
      year: this.certificateYear,
    };

    this.taxDocumentsApi.generateCertificate(request, this.certificateRegenerate).subscribe({
      next: document => {
        this.certificateResult = document;
        this.isLoading = false;
        this.loadHistory();
      },
      error: err => {
        this.errorMessage = err.error?.message || 'Unable to generate certificate.';
        this.isLoading = false;
      },
    });
  }

  downloadDocument(document: TaxDocumentHistory | TaxDocumentMetadata, fallbackName: string): void {
    this.taxDocumentsApi.downloadDocument(document.id).subscribe({
      next: file => this.saveBase64File(file, fallbackName),
      error: err => console.error('Failed to download tax document', err),
    });
  }

  private saveBase64File(file: FileExportResult, fallbackName: string): void {
    const binary = atob(file.contentBase64);
    const array = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
      array[i] = binary.charCodeAt(i);
    }

    const blob = new Blob([array], { type: file.contentType || 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = file.fileName || fallbackName;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
