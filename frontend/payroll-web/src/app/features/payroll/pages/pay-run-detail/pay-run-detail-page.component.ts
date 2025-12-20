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
import { PayslipDocument, PayslipDocumentStatus } from '../../models/payslip-document.model';
import { PaySlipEarningLine } from '../../models/payslip.model';
import { GlJournalBatchDetail, GlJournalBatchSummary } from '../../models/gl.model';
import { TimeReconciliationResult } from '../../models/time-reconciliation.model';
import { PayRunsApiService } from '../../services/pay-runs-api.service';
import { GlApiService } from '../../services/gl-api.service';

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
  glBatches: GlJournalBatchSummary[] = [];
  selectedGlBatch?: GlJournalBatchDetail;
  glErrorMessage: string | null = null;
  isLoadingGl = false;
  isGeneratingGl = false;
  isApprovingGl = false;
  isExportingGl = false;
  selectedTemplateId: string | null = null;
  isLoadingExports = false;
  apitReport?: ApitReport;
  isLoadingApit = false;
  apitError: string | null = null;
  downloadingCertificateId: string | null = null;
  payslipDocuments: PayslipDocument[] = [];
  isLoadingPayslipDocuments = false;
  isGeneratingPayslips = false;
  generatingPayslipEmployeeId: string | null = null;
  payslipDocumentsMessage: string | null = null;
  timeReconciliation?: TimeReconciliationResult;
  isLoadingTimeReconciliation = false;
  timeReconciliationError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private payRunsApi: PayRunsApiService,
    private glApi: GlApiService,
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
        this.loadGlBatches();
        this.loadPayslipDocuments();
        this.loadTimeReconciliation();
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

  loadGlBatches(): void {
    if (!this.payRun) {
      return;
    }

    this.isLoadingGl = true;
    this.glErrorMessage = null;
    this.glApi.getPayRunBatches(this.payRun.id).subscribe({
      next: batches => {
        this.glBatches = batches;
        if (this.selectedGlBatch) {
          const existing = batches.find(batch => batch.id === this.selectedGlBatch?.id);
          if (!existing) {
            this.selectedGlBatch = undefined;
          }
        }
        this.isLoadingGl = false;
      },
      error: err => {
        console.error('Failed to load GL batches', err);
        this.glErrorMessage = err.error?.message || 'Failed to load GL batches.';
        this.isLoadingGl = false;
      },
    });
  }

  generateGlBatch(regenerate = false): void {
    if (!this.payRun) {
      return;
    }

    this.isGeneratingGl = true;
    this.glErrorMessage = null;
    this.glApi.generateBatch(this.payRun.id, regenerate).subscribe({
      next: batch => {
        this.selectedGlBatch = batch;
        this.isGeneratingGl = false;
        this.loadGlBatches();
      },
      error: err => {
        console.error('Failed to generate GL batch', err);
        this.glErrorMessage = err.error?.message || 'Failed to generate GL batch.';
        this.isGeneratingGl = false;
      },
    });
  }

  viewGlBatch(batchId: string): void {
    this.glErrorMessage = null;
    this.glApi.getBatch(batchId).subscribe({
      next: batch => {
        this.selectedGlBatch = batch;
      },
      error: err => {
        console.error('Failed to load GL batch', err);
        this.glErrorMessage = err.error?.message || 'Failed to load GL batch details.';
      },
    });
  }

  approveGlBatch(): void {
    if (!this.selectedGlBatch) {
      return;
    }

    this.isApprovingGl = true;
    this.glErrorMessage = null;
    this.glApi.approveBatch(this.selectedGlBatch.id, { comment: this.actionComment }).subscribe({
      next: batch => {
        this.selectedGlBatch = batch;
        this.isApprovingGl = false;
        this.loadGlBatches();
      },
      error: err => {
        console.error('Failed to approve GL batch', err);
        this.glErrorMessage = err.error?.message || 'Failed to approve GL batch.';
        this.isApprovingGl = false;
      },
    });
  }

  exportGlBatch(): void {
    if (!this.selectedGlBatch) {
      return;
    }

    this.isExportingGl = true;
    this.glErrorMessage = null;
    this.glApi.exportBatch(this.selectedGlBatch.id).subscribe({
      next: file => {
        this.saveBase64File(file, 'gl-export.csv');
        this.isExportingGl = false;
        this.loadGlBatches();
      },
      error: err => {
        console.error('Failed to export GL batch', err);
        this.glErrorMessage = err.error?.message || 'Failed to export GL batch.';
        this.isExportingGl = false;
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

  loadPayslipDocuments(): void {
    if (!this.payRun) {
      return;
    }

    this.isLoadingPayslipDocuments = true;
    this.payslipDocumentsMessage = null;

    this.payRunsApi.getPayslipDocuments(this.payRun.id).subscribe({
      next: documents => {
        this.payslipDocuments = documents;
        this.isLoadingPayslipDocuments = false;
      },
      error: err => {
        console.error('Failed to load payslip documents', err);
        this.payslipDocumentsMessage = 'Unable to load payslip documents.';
        this.isLoadingPayslipDocuments = false;
      },
    });
  }

  loadTimeReconciliation(): void {
    if (!this.payRun) {
      return;
    }

    this.isLoadingTimeReconciliation = true;
    this.timeReconciliationError = null;

    this.payRunsApi.getTimeReconciliation(this.payRun.id).subscribe({
      next: result => {
        this.timeReconciliation = result;
        this.isLoadingTimeReconciliation = false;
      },
      error: err => {
        console.error('Failed to load time reconciliation', err);
        this.timeReconciliationError = 'Unable to load time reconciliation summary.';
        this.isLoadingTimeReconciliation = false;
      },
    });
  }

  generateAllPayslips(): void {
    if (!this.payRun) {
      return;
    }

    if (!confirm('Generate payslip PDFs for all employees?')) {
      return;
    }

    this.isGeneratingPayslips = true;
    this.payslipDocumentsMessage = null;

    this.payRunsApi.generatePayslipDocumentsBulk(this.payRun.id).subscribe({
      next: result => {
        this.isGeneratingPayslips = false;
        this.payslipDocumentsMessage = `Generated ${result.generatedCount}, skipped ${result.skippedCount}, failed ${result.failedCount}.`;
        this.loadPayslipDocuments();
      },
      error: err => {
        console.error('Failed to generate payslip documents', err);
        this.isGeneratingPayslips = false;
        this.payslipDocumentsMessage = err.error?.message || 'Failed to generate payslip documents.';
      },
    });
  }

  generatePayslipForEmployee(employeeId: string, regenerate: boolean): void {
    if (!this.payRun) {
      return;
    }

    this.generatingPayslipEmployeeId = employeeId;
    this.payslipDocumentsMessage = null;

    this.payRunsApi.generatePayslipDocument(this.payRun.id, employeeId, regenerate).subscribe({
      next: () => {
        this.generatingPayslipEmployeeId = null;
        this.loadPayslipDocuments();
      },
      error: err => {
        console.error('Failed to generate payslip document', err);
        this.generatingPayslipEmployeeId = null;
        this.payslipDocumentsMessage = err.error?.message || 'Failed to generate payslip document.';
      },
    });
  }

  downloadPayslipDocument(documentId: string): void {
    this.payRunsApi.downloadPayslipDocument(documentId).subscribe({
      next: file => {
        this.saveBase64File(file, 'payslip.pdf');
      },
      error: err => {
        console.error('Failed to download payslip document', err);
        this.payslipDocumentsMessage = err.error?.message || 'Failed to download payslip document.';
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

  getLatestPayslipDocument(employeeId: string): PayslipDocument | undefined {
    return this.payslipDocuments.find(doc => doc.employeeId === employeeId);
  }

  getOvertimeEarningsForSlip(paySlipId: string): PaySlipEarningLine[] {
    const slip = this.payRun?.paySlips.find(ps => ps.id === paySlipId);
    return slip?.earnings?.filter(e => e.code === 'OT') ?? [];
  }

  hasOvertimeEarnings(): boolean {
    return this.payRun?.paySlips.some(slip => this.getOvertimeEarningsForSlip(slip.id).length > 0) ?? false;
  }

  getNoPayAmount(employeeId: string): number {
    const slip = this.payRun?.paySlips.find(ps => ps.employeeId === employeeId);
    return slip?.deductions?.filter(d => d.code === 'DED_NO_PAY').reduce((sum, line) => sum + line.amount, 0) ?? 0;
  }

  getLeaveEncashmentAmount(employeeId: string): number {
    const slip = this.payRun?.paySlips.find(ps => ps.employeeId === employeeId);
    return slip?.earnings?.filter(e => e.code === 'LEAVE_ENCASHMENT').reduce((sum, line) => sum + line.amount, 0) ?? 0;
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

  get canGeneratePayslips(): boolean {
    return !!this.payRun && this.payRun.isLocked && this.payRun.status === 'Locked';
  }

  get canGenerateGlBatch(): boolean {
    return !!this.payRun && this.payRun.isLocked && this.payRun.status === 'Locked';
  }

  getStatusClass(status?: PayslipDocumentStatus): string {
    if (!status) {
      return 'status-pending';
    }

    switch (status) {
      case 'Generated':
        return 'status-generated';
      case 'Failed':
        return 'status-failed';
      default:
        return 'status-pending';
    }
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
