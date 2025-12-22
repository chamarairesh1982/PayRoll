import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { SharedModule } from '../../shared/shared.module';
import { OrganizationApiService } from '../../shared/services/organization-api.service';
import { PageHeaderComponent } from '../../shared/ui/page-header/page-header.component';
import { EmptyStateComponent } from '../../shared/ui/empty-state/empty-state.component';
import { ErrorBannerComponent } from '../../shared/ui/error-banner/error-banner.component';
import { SkeletonBlockComponent } from '../../shared/ui/skeleton-block/skeleton-block.component';
import { BranchOption, CompanyOption, CostCenterOption } from '../../shared/models/organization.model';
import { DashboardApiService } from './services/dashboard-api.service';
import { DashboardActivity, DashboardSummary } from './models/dashboard-summary.model';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    SharedModule,
    PageHeaderComponent,
    EmptyStateComponent,
    ErrorBannerComponent,
    SkeletonBlockComponent,
  ],
  templateUrl: './dashboard-page.component.html',
  styleUrls: ['./dashboard-page.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent implements OnInit, OnDestroy {
  summary?: DashboardSummary;
  errorMessage: string | null = null;
  isLoading = false;

  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];

  activityColumns = [
    { field: 'occurredAt', header: 'Timestamp', type: 'datetime', sortable: true, filter: true },
    { field: 'activity', header: 'Activity', sortable: true, filter: true },
    { field: 'actor', header: 'Actor', sortable: true, filter: true },
    { field: 'context', header: 'Tenant Scope', sortable: true, filter: true },
  ];
  selectedActivityColumns = [...this.activityColumns];

  filtersForm = new FormGroup({
    periodStart: new FormControl<Date | null>(this.startOfCurrentMonth()),
    periodEnd: new FormControl<Date | null>(this.endOfCurrentMonth()),
    companyId: new FormControl<string | null>(null),
    branchId: new FormControl<string | null>(null),
    costCenterId: new FormControl<string | null>(null),
  });

  private destroy$ = new Subject<void>();

  constructor(
    private dashboardApi: DashboardApiService,
    private organizationApi: OrganizationApiService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    this.loadOrganizations();
    this.loadSummary();

    this.filtersForm
      .get('companyId')
      ?.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(companyId => {
        this.filtersForm.patchValue({ branchId: null, costCenterId: null }, { emitEvent: false });
        this.loadBranches(companyId);
        this.loadCostCenters(companyId, null);
      });

    this.filtersForm
      .get('branchId')
      ?.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(branchId => {
        const companyId = this.filtersForm.get('companyId')?.value ?? undefined;
        this.filtersForm.patchValue({ costCenterId: null }, { emitEvent: false });
        this.loadCostCenters(companyId, branchId);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  applyFilters(): void {
    this.loadSummary();
  }

  trackByHealth = (_: number, item: { key: string }) => item.key;

  trackByActivity = (index: number, item: DashboardActivity) => item.id ?? index;

  get healthItems(): Array<{
    key: string;
    label: string;
    description: string;
    route: string;
    count: number;
    severity: 'success' | 'warning' | 'danger';
  }> {
    const health = this.summary?.health;
    return [
      {
        key: 'MISSING_EPF',
        label: 'Missing EPF No',
        description: 'Employees without EPF registration',
        route: '/people/employees',
        count: health?.missingEpf ?? 0,
        severity: 'warning',
      },
      {
        key: 'MISSING_BANK',
        label: 'Missing Bank Details',
        description: 'Employees without bank transfer setup',
        route: '/people/employees',
        count: health?.missingBank ?? 0,
        severity: 'warning',
      },
      {
        key: 'PENDING_ATTENDANCE',
        label: 'Pending Attendance',
        description: 'Attendance not submitted for the period',
        route: '/time/attendance',
        count: health?.pendingAttendance ?? 0,
        severity: 'danger',
      },
      {
        key: 'PENDING_OT',
        label: 'Pending OT',
        description: 'Overtime entries awaiting approval',
        route: '/overtime',
        count: health?.pendingOt ?? 0,
        severity: 'danger',
      },
      {
        key: 'NEGATIVE_NET',
        label: 'Negative Net Pay',
        description: 'Payslips with negative net pay',
        route: '/payroll/pay-runs',
        count: health?.negativeNet ?? 0,
        severity: 'danger',
      },
    ];
  }

  private loadSummary(): void {
    this.isLoading = true;
    this.errorMessage = null;
    const formValue = this.filtersForm.value;

    this.dashboardApi
      .getSummary({
        periodStart: this.formatDate(formValue.periodStart),
        periodEnd: this.formatDate(formValue.periodEnd),
        companyId: formValue.companyId ?? undefined,
        branchId: formValue.branchId ?? undefined,
        costCenterId: formValue.costCenterId ?? undefined,
      })
      .subscribe({
        next: summary => {
          this.summary = summary;
          this.selectedActivityColumns = [...this.activityColumns];
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: err => {
          console.error('Failed to load dashboard summary', err);
          this.errorMessage = 'Unable to load dashboard data. Please retry.';
          this.isLoading = false;
          this.cdr.markForCheck();
        },
      });
  }

  private loadOrganizations(): void {
    this.organizationApi.getCompanies().pipe(takeUntil(this.destroy$)).subscribe({
      next: companies => {
        this.companies = companies;
        this.cdr.markForCheck();
      },
      error: err => {
        console.error('Failed to load companies', err);
      },
    });
  }

  private loadBranches(companyId?: string | null): void {
    this.organizationApi.getBranches(companyId ?? undefined).pipe(takeUntil(this.destroy$)).subscribe({
      next: branches => {
        this.branches = branches;
        this.cdr.markForCheck();
      },
      error: err => {
        console.error('Failed to load branches', err);
      },
    });
  }

  private loadCostCenters(companyId?: string | null, branchId?: string | null): void {
    this.organizationApi
      .getCostCenters(companyId ?? undefined, branchId ?? undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: costCenters => {
          this.costCenters = costCenters;
          this.cdr.markForCheck();
        },
        error: err => {
          console.error('Failed to load cost centers', err);
        },
      });
  }

  private startOfCurrentMonth(): Date {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), 1);
  }

  private endOfCurrentMonth(): Date {
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth() + 1, 0);
  }

  private formatDate(value: Date | null | undefined): string | undefined {
    if (!value) {
      return undefined;
    }
    return value.toISOString().split('T')[0];
  }
}
