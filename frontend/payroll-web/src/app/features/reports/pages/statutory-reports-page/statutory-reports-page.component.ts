import { Component, OnInit } from '@angular/core';
import { PayRunSummary } from '../../../payroll/models/pay-run.model';
import { PayRunsApiService } from '../../../payroll/services/pay-runs-api.service';
import { StatutoryReportsApiService } from '../../services/statutory-reports-api.service';
import {
  EpfEtfReportResult,
  FileExportResult,
  StatutoryReportHistory,
} from '../../models/statutory-report.model';

@Component({
  selector: 'app-statutory-reports-page',
  templateUrl: './statutory-reports-page.component.html',
  styleUrls: ['./statutory-reports-page.component.scss'],
})
export class StatutoryReportsPageComponent implements OnInit {
  payRuns: PayRunSummary[] = [];
  selectedPayRunId = '';
  format: 'csv' | 'txt' = 'csv';
  isLoadingPayRuns = false;
  isGenerating = false;
  errorMessage: string | null = null;
  reportHistory: StatutoryReportHistory[] = [];
  reportPreview: EpfEtfReportResult | null = null;

  constructor(
    private payRunsApi: PayRunsApiService,
    private reportsApi: StatutoryReportsApiService,
  ) {}

  ngOnInit(): void {
    this.loadPayRuns();
    this.loadHistory();
  }

  loadPayRuns(): void {
    this.isLoadingPayRuns = true;
    this.payRunsApi
      .getPayRuns({ page: 1, pageSize: 200, status: 'Locked' })
      .subscribe({
        next: result => {
          this.payRuns = result.items;
          this.isLoadingPayRuns = false;
        },
        error: err => {
          console.error('Failed to load locked pay runs', err);
          this.isLoadingPayRuns = false;
        },
      });
  }

  loadHistory(): void {
    this.reportsApi.getReports({ type: 'EpfEtf', payRunId: this.selectedPayRunId || undefined }).subscribe({
      next: history => {
        this.reportHistory = history;
      },
      error: err => console.error('Failed to load report history', err),
    });
  }

  onPayRunChange(payRunId: string): void {
    this.selectedPayRunId = payRunId;
    this.loadHistory();
  }

  generateReport(): void {
    if (!this.selectedPayRunId) {
      this.errorMessage = 'Select a locked pay run before generating the report.';
      return;
    }

    this.isGenerating = true;
    this.errorMessage = null;

    this.reportsApi.generateEpfEtfReport(this.selectedPayRunId, this.format).subscribe({
      next: report => {
        this.reportPreview = report;
        this.isGenerating = false;
        this.loadHistory();
      },
      error: err => {
        console.error('Failed to generate EPF/ETF report', err);
        this.errorMessage = err.error?.message || 'Unable to generate EPF/ETF report.';
        this.isGenerating = false;
      },
    });
  }

  downloadHistory(report: StatutoryReportHistory, warnings = false): void {
    this.reportsApi.downloadReport(report.id, warnings).subscribe({
      next: file => this.saveBase64File(file, warnings ? 'epf-etf-warnings.csv' : 'epf-etf-report.csv'),
      error: err => console.error('Failed to download report file', err),
    });
  }

  downloadPreview(file: FileExportResult, fallbackName: string): void {
    this.saveBase64File(file, fallbackName);
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
