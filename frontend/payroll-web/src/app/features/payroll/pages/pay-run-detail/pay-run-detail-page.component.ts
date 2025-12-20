import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { ApitReport, FileExportResult } from '../../models/apit-report.model';
import {
  BankExportTemplate,
  BankExportValidationError,
  PayRunBankExport,
} from '../../models/bank-export.model';
import { PayRunDetail } from '../../models/pay-run.model';
import { PaySlipEarningLine } from '../../models/payslip.model';
import { PayRunsApiService } from '../../services/pay-runs-api.service';

@Component({
  selector: 'app-pay-run-detail-page',
  templateUrl: './pay-run-detail-page.component.html',
  styleUrls: ['./pay-run-detail-page.component.scss'],
})
export class PayRunDetailPageComponent implements OnInit {
  payRun?: PayRunDetail;
  isLoading = true;
  isRecalculating = false;
  isChangingStatus = false;
  isExporting = false;
  errorMessage: string | null = null;
  actionComment = '';
  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];
  bankExportTemplates: BankExportTemplate[] = [];
  bankExports: PayRunBankExport[] = [];
  bankExportErrors: BankExportValidationError[] = [];
  selectedTemplateId: string | null = null;
  isLoadingExports = false;
  apitReport?: ApitReport;
  isLoadingApit = false;
  apitError: string | null = null;
  downloadingCertificateId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private payRunsApi: PayRunsApiService,
    private organizationApi: OrganizationApiService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadScopeOptions();
    this.loadPayRun();
    this.loadBankExportTemplates();
  }

  loadScopeOptions(): void {
    this.organizationApi.getCompanies().subscribe(companies => (this.companies = companies));
    this.organizationApi.getBranches().subscribe(branches => (this.branches = branches));
    this.organizationApi.getCostCenters().subscribe(costCenters => (this.costCenters = costCenters));
  }

  loadPayRun(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = null;
    this.payRunsApi.getPayRun(id).subscribe({
      next: payRun => {
        this.payRun = payRun;
        this.isLoading = false;
        this.loadBankExports();
      },
      error: err => {
        console.error('Failed to load pay run', err);
        this.errorMessage = 'Failed to load pay run. Please try again later.';
        this.isLoading = false;
      },
    });
  }

  loadBankExportTemplates(): void {
    this.payRunsApi.getBankExportTemplates().subscribe({
      next: templates => {
        this.bankExportTemplates = templates;
        if (!this.selectedTemplateId && templates.length) {
          this.selectedTemplateId = templates[0].id;
        }
      },
      error: err => {
        console.error('Failed to load bank export templates', err);
      },
    });
  }

  loadBankExports(): void {
    if (!this.payRun) {
      return;
    }

    this.isLoadingExports = true;
    this.payRunsApi.getPayRunBankExports(this.payRun.id).subscribe({
      next: exports => {
        this.bankExports = exports;
        this.isLoadingExports = false;
      },
      error: err => {
        console.error('Failed to load bank exports', err);
        this.isLoadingExports = false;
      },
    });
  }

  recalculate(): void {
    if (!this.payRun || this.payRun.isLocked) {
      return;
    }

    if (!confirm('Recalculate this pay run?')) {
      return;
    }

    this.isRecalculating = true;
    this.payRunsApi
      .recalculatePayRun(this.payRun.id, {
        includeAttendance: true,
        includeOvertime: true,
        includeLoans: true,
        includeAllowancesAndDeductions: true,
      })
      .subscribe({
        next: () => {
          this.isRecalculating = false;
          this.loadPayRun();
        },
        error: err => {
          console.error('Failed to recalculate pay run', err);
          this.isRecalculating = false;
        },
      });
  }

  prepare(): void {
    if (!this.payRun || this.payRun.status !== 'Draft') {
      return;
    }

    this.isChangingStatus = true;
    this.errorMessage = null;

    this.payRunsApi.preparePayRun(this.payRun.id, this.actionComment ? { comment: this.actionComment } : {}).subscribe({
      next: () => {
        this.isChangingStatus = false;
        this.actionComment = '';
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to prepare pay run', err);
        this.errorMessage = err.error?.message || 'Failed to prepare pay run.';
        this.isChangingStatus = false;
      },
    });
  }

  approve(): void {
    if (!this.payRun || this.payRun.status !== 'Prepared') {
      return;
    }

    if (!this.actionComment.trim()) {
      this.errorMessage = 'Approval comment is required.';
      return;
    }

    this.isChangingStatus = true;
    this.errorMessage = null;

    this.payRunsApi.approvePayRun(this.payRun.id, { comment: this.actionComment }).subscribe({
      next: () => {
        this.isChangingStatus = false;
        this.actionComment = '';
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to approve pay run', err);
        this.errorMessage = err.error?.message || 'Failed to approve pay run.';
        this.isChangingStatus = false;
      },
    });
  }

  lock(): void {
    if (!this.payRun || this.payRun.status !== 'Approved') {
      return;
    }

    if (!this.actionComment.trim()) {
      this.errorMessage = 'Lock comment is required.';
      return;
    }

    this.isChangingStatus = true;
    this.errorMessage = null;

    this.payRunsApi.lockPayRun(this.payRun.id, { comment: this.actionComment }).subscribe({
      next: () => {
        this.isChangingStatus = false;
        this.actionComment = '';
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to lock pay run', err);
        this.errorMessage = err.error?.message || 'Failed to lock pay run.';
        this.isChangingStatus = false;
      },
    });
  }

  generateBankExport(): void {
    if (!this.payRun || !this.selectedTemplateId) {
      return;
    }

    this.isExporting = true;
    this.errorMessage = null;
    this.bankExportErrors = [];

    this.payRunsApi.generateBankExport(this.payRun.id, this.selectedTemplateId).subscribe({
      next: result => {
        this.isExporting = false;

        if (result.validationErrors?.length) {
          this.bankExportErrors = result.validationErrors;
        }

        this.loadBankExports();
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to generate bank export', err);
        this.errorMessage = err.error?.message || 'Failed to generate bank export file.';
        this.isExporting = false;
      },
    });
  }

  downloadExport(exportRecord: PayRunBankExport): void {
    this.payRunsApi.downloadBankExport(exportRecord.id).subscribe({
      next: file => {
        this.saveBase64File(file, 'bank-export.txt');
        this.loadBankExports();
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to download bank export', err);
        this.errorMessage = err.error?.message || 'Failed to download bank export file.';
      },
    });
  }

  downloadExportErrors(exportRecord: PayRunBankExport): void {
    this.payRunsApi.downloadBankExportErrors(exportRecord.id).subscribe({
      next: file => {
        this.saveBase64File(file, 'bank-export-errors.csv');
      },
      error: err => {
        console.error('Failed to download bank export errors', err);
        this.errorMessage = err.error?.message || 'Failed to download bank export errors.';
      },
    });
  }

  loadApitReport(): void {
    if (!this.payRun) {
      return;
    }

    this.isLoadingApit = true;
    this.apitError = null;

    this.payRunsApi.getApitReport(this.payRun.id).subscribe({
      next: report => {
        this.apitReport = report;
        this.isLoadingApit = false;
      },
      error: err => {
        console.error('Failed to load APIT report', err);
        this.apitError = 'Unable to load APIT report for this pay run.';
        this.isLoadingApit = false;
      },
    });
  }

  downloadApitCertificate(paySlipId: string): void {
    if (!this.payRun) {
      return;
    }

    this.downloadingCertificateId = paySlipId;
    this.payRunsApi.downloadApitCertificate(this.payRun.id, paySlipId).subscribe({
      next: file => {
        this.saveBase64File(file, 'apit-certificate.txt');
        this.downloadingCertificateId = null;
      },
      error: err => {
        console.error('Failed to download APIT certificate', err);
        this.downloadingCertificateId = null;
      },
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

  viewPaySlip(paySlipId: string): void {
    if (!this.payRun) {
      return;
    }

    this.router.navigate(['/payroll', this.payRun.id, 'payslips', paySlipId]);
  }

  getOvertimeEarningsForSlip(paySlipId: string): PaySlipEarningLine[] {
    const slip = this.payRun?.paySlips.find(ps => ps.id === paySlipId);
    return slip?.earnings?.filter(e => e.code === 'OT') ?? [];
  }

  get canRecalculate(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'Draft';
  }

  get canPrepare(): boolean {
    return !!this.payRun && this.payRun.status === 'Draft';
  }

  get canApprove(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'Prepared';
  }

  get canLock(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'Approved';
  }

  get canGenerateBankExport(): boolean {
    return !!this.payRun && this.payRun.isLocked && this.payRun.status === 'Locked';
  }

  get scopeLabel(): string {
    if (!this.payRun) {
      return '';
    }

    if (this.payRun.isConsolidated) {
      return 'Consolidated';
    }

    const companyLabel =
      this.payRun.companyId && this.companies.find(c => c.id === this.payRun?.companyId)?.name;
    const branchLabel = this.payRun.branchId && this.branches.find(b => b.id === this.payRun?.branchId)?.name;
    const costCenterLabel =
      this.payRun.costCenterId && this.costCenters.find(c => c.id === this.payRun?.costCenterId)?.name;

    const segments = [
      companyLabel || (this.payRun.companyId ? `Company ${this.payRun.companyId}` : null),
      branchLabel || (this.payRun.branchId ? `Branch ${this.payRun.branchId}` : null),
      costCenterLabel || (this.payRun.costCenterId ? `Cost Center ${this.payRun.costCenterId}` : null),
    ].filter(Boolean);

    return segments.length ? segments.join(' / ') : 'Unscoped';
  }
}
