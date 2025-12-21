import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';
import { ExportToolbarComponent } from '../../components/export-toolbar/export-toolbar.component';
import {
  ReportDefinition,
  ReportFilterOptions,
  ReportFilters,
  ReportKey,
  ReportResult,
} from '../../models/report.models';
import { ReportService } from '../../services/report.service';

@Component({
  selector: 'app-report-viewer',
  templateUrl: './report-viewer.component.html',
  styleUrls: ['./report-viewer.component.scss'],
})
export class ReportViewerComponent implements OnInit, OnDestroy {
  @ViewChild(ExportToolbarComponent) exportToolbar?: ExportToolbarComponent;

  reportKey: ReportKey = 'epf-etf';
  reportDefinition: ReportDefinition | null = null;
  reportResult: ReportResult | null = null;
  filterOptions: ReportFilterOptions | null = null;
  filters: ReportFilters | null = null;
  isLoading = false;
  isPrintView = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const paramKey = params.get('reportKey') as ReportKey | null;
      this.reportKey = paramKey ?? 'epf-etf';
      this.reportDefinition = this.reportService.getReportDefinition(this.reportKey);
      this.filters = this.buildDefaultFilters();
      this.filterOptions = this.reportService.getFilterOptions();
      this.isPrintView = !!this.route.snapshot.data['print'];

      if (this.isPrintView) {
        const cached = this.reportService.getCachedReport(this.reportKey);
        if (cached) {
          this.reportResult = cached;
        } else {
          this.generateReport(this.filters);
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onGenerate(filters: ReportFilters): void {
    this.filters = filters;
    this.generateReport(filters);
  }

  openPrintView(): void {
    void this.router.navigate(['/statutory-reports/print', this.reportKey]);
  }

  private generateReport(filters: ReportFilters | null): void {
    if (!filters) {
      return;
    }
    const generatedBy = this.authService.getUserName() || 'Payroll Admin';
    this.isLoading = true;
    this.reportService
      .generateReport(this.reportKey, filters, generatedBy)
      .pipe(
        finalize(() => {
          this.isLoading = false;
        }),
        takeUntil(this.destroy$),
      )
      .subscribe(result => {
        this.reportResult = result;
        if (filters.outputMode === 'export' && filters.outputFormat === 'csv') {
          setTimeout(() => this.exportToolbar?.export('csv'));
        }
        if (filters.outputMode === 'export' && filters.outputFormat === 'pdf') {
          this.openPrintView();
        }
      });
  }

  private buildDefaultFilters(): ReportFilters {
    const now = new Date();
    const month = new Date(now.getFullYear(), now.getMonth(), 1);
    return {
      companyId: null,
      branchId: null,
      costCenterId: null,
      payrollMonth: month,
      employeeStatus: 'active',
      outputMode: 'preview',
      outputFormat: 'csv',
    };
  }
}
