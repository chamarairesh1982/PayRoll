import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { ApitReport, FileExportResult } from '../../models/apit-report.model';
import { BankExportFailure, BankExportResult, PayRunDetail } from '../../models/pay-run.model';
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
  bankExportFailures: BankExportFailure[] = [];
  selectedBank = 'HNB';
  bankOptions = ['HNB', 'BOC', 'Commercial'];
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
      },
      error: err => {
        console.error('Failed to load pay run', err);
        this.errorMessage = 'Failed to load pay run. Please try again later.';
        this.isLoading = false;
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
    if (!this.payRun) {
      return;
    }

    this.isExporting = true;
    this.errorMessage = null;
    this.bankExportFailures = [];

    this.payRunsApi.generateBankExport(this.payRun.id, this.selectedBank).subscribe({
      next: result => {
        this.isExporting = false;

        if (result.failures?.length) {
          this.bankExportFailures = result.failures;
          return;
        }

        this.downloadExport(result);
      },
      error: err => {
        console.error('Failed to generate bank export', err);
        this.errorMessage = err.error?.message || 'Failed to generate bank export file.';
        this.isExporting = false;
      },
    });
  }

  private downloadExport(result: BankExportResult): void {
    this.saveBase64File(result, 'bank-export.txt');

    this.payRunsApi.markBankExportDownloaded(this.payRun!.id).subscribe({
      next: () => this.loadPayRun(),
      error: err => console.warn('Failed to mark export downloaded', err),
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
