import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { BranchOption, CompanyOption, CostCenterOption } from '../../../../shared/models/organization.model';
import { OrganizationApiService } from '../../../../shared/services/organization-api.service';
import { PayRunStatus, PayRunSummary } from '../../models/pay-run.model';
import { PayRunsApiService } from '../../services/pay-runs-api.service';
import { DataTableColumn } from '../../../../shared/components/table/data-table.component';

type PayRunRow = PayRunSummary & { period: string; scope: string };

@Component({
  selector: 'app-pay-runs-list-page',
  templateUrl: './pay-runs-list-page.component.html',
  styleUrls: ['./pay-runs-list-page.component.scss'],
})
export class PayRunsListPageComponent implements OnInit {
  payRuns: PayRunRow[] = [];
  page = 1;
  pageSize = 20;
  totalCount = 0;
  isLoading = false;

  columns: DataTableColumn<PayRunRow>[] = [
    { field: 'code', header: 'Code', sortable: true, minWidth: '100px' },
    { field: 'name', header: 'Name', sortable: true, minWidth: '150px' },
    { field: 'period', header: 'Period', minWidth: '200px' },
    { field: 'payDate', header: 'Pay Date', type: 'date', sortable: true, minWidth: '120px' },
    { field: 'employeeCount', header: 'Employees', align: 'center', sortable: true, minWidth: '100px' },
    { field: 'totalNetPay', header: 'Total Net Pay', type: 'currency', align: 'right', sortable: true, minWidth: '140px' },
    { field: 'status', header: 'Status', type: 'badge', align: 'center', sortable: true, minWidth: '100px' },
  ];

  selectedStatus: PayRunStatus | '' = '';
  companyFilter: string = '';
  branchFilter: string = '';
  costCenterFilter: string = '';
  consolidatedFilter: '' | 'true' | 'false' = '';

  companies: CompanyOption[] = [];
  branches: BranchOption[] = [];
  costCenters: CostCenterOption[] = [];

  statusOptions = [
    { label: 'All Statuses', value: '' },
    { label: 'Draft', value: 'Draft' },
    { label: 'Prepared', value: 'Prepared' },
    { label: 'Approved', value: 'Approved' },
    { label: 'Locked', value: 'Locked' },
  ];

  constructor(
    private payRunsApi: PayRunsApiService,
    private organizationApi: OrganizationApiService,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.loadCompanies();
    this.load();
  }

  loadCompanies(): void {
    this.organizationApi.getCompanies().subscribe(companies => {
      this.companies = companies;
      this.loadBranches();
      this.loadCostCenters();
    });
  }

  loadBranches(): void {
    this.organizationApi.getBranches(this.companyFilter || undefined).subscribe(branches => {
      this.branches = branches;
    });
  }

  loadCostCenters(): void {
    this.organizationApi
      .getCostCenters(this.companyFilter || undefined, this.branchFilter || undefined)
      .subscribe(costCenters => {
        this.costCenters = costCenters;
      });
  }

  load(): void {
    this.isLoading = true;
    this.payRunsApi
      .getPayRuns({
        page: this.page,
        pageSize: this.pageSize,
        status: this.selectedStatus || undefined,
        companyId: this.companyFilter || undefined,
        branchId: this.branchFilter || undefined,
        costCenterId: this.costCenterFilter || undefined,
        isConsolidated:
          this.consolidatedFilter === '' ? undefined : this.consolidatedFilter === 'true' ? true : false,
      })
      .subscribe({
        next: (result: PaginatedResult<PayRunSummary>) => {
          this.payRuns = result.items.map(pr => ({
            ...pr,
            period: `${pr.periodStart} - ${pr.periodEnd}`,
            scope: this.buildScopeLabel(pr),
          }));
          this.totalCount = result.totalCount;
          this.page = result.page;
          this.pageSize = result.pageSize;
          this.isLoading = false;
        },
        error: err => {
          console.error('Failed to load pay runs', err);
          this.isLoading = false;
        },
      });
  }

  onStatusChange(): void {
    this.page = 1;
    this.load();
  }

  onScopeChange(): void {
    this.page = 1;
    this.load();
  }

  onCompanyChange(value: string): void {
    this.companyFilter = value;
    this.branchFilter = '';
    this.costCenterFilter = '';
    this.loadBranches();
    this.loadCostCenters();
    this.onScopeChange();
  }

  onBranchChange(value: string): void {
    this.branchFilter = value;
    this.costCenterFilter = '';
    this.loadCostCenters();
    this.onScopeChange();
  }

  onCostCenterChange(value: string): void {
    this.costCenterFilter = value;
    this.onScopeChange();
  }

  goToCreate(): void {
    this.router.navigate(['/payroll/new']);
  }

  viewPayRun(payRun: PayRunSummary): void {
    this.router.navigate(['/payroll', payRun.id]);
  }

  onLazyLoad(event: any): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? 20;
    const newPage = Math.floor(first / rows) + 1;

    if (newPage !== this.page || rows !== this.pageSize) {
      this.page = newPage;
      this.pageSize = rows;
      this.load();
    }
  }

  nextPage(): void {
    if (this.page * this.pageSize < this.totalCount) {
      this.page++;
      this.load();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }

  private buildScopeLabel(payRun: PayRunSummary): string {
    if (payRun.isConsolidated) {
      return 'Consolidated';
    }

    const companyLabel =
      payRun.companyId && this.companies.find(c => c.id === payRun.companyId)?.name;
    const branchLabel = payRun.branchId && this.branches.find(b => b.id === payRun.branchId)?.name;
    const costCenterLabel =
      payRun.costCenterId && this.costCenters.find(c => c.id === payRun.costCenterId)?.name;

    const segments = [
      companyLabel || (payRun.companyId ? `Company ${payRun.companyId}` : null),
      branchLabel || (payRun.branchId ? `Branch ${payRun.branchId}` : null),
      costCenterLabel || (payRun.costCenterId ? `Cost Center ${payRun.costCenterId}` : null),
    ].filter(Boolean);

    return segments.length ? segments.join(' / ') : 'Unscoped';
  }
}
