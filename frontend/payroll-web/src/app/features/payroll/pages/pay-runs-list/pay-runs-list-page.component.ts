import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PaginatedResult } from '../../../employees/models/employee.model';
import { PayRunStatus, PayRunSummary } from '../../models/pay-run.model';
import { PayRunsApiService } from '../../services/pay-runs-api.service';

type PayRunRow = PayRunSummary & { period: string };

@Component({
  selector: 'app-pay-runs-list-page',
  templateUrl: './pay-runs-list-page.component.html',
  styleUrls: ['./pay-runs-list-page.component.scss'],
})
export class PayRunsListPageComponent implements OnInit {
  payRuns: PayRunRow[] = [];
  page = 1;
  pageSize = 25;
  totalCount = 0;
  isLoading = false;
  selectedStatus: PayRunStatus | '' = '';

  statusOptions: (PayRunStatus | '')[] = ['', 'Draft', 'Calculated', 'UnderReview', 'Approved', 'Posted', 'Cancelled'];

  columns: { field: keyof PayRunRow; header: string }[] = [
    { field: 'code', header: 'Code' },
    { field: 'name', header: 'Name' },
    { field: 'period', header: 'Period' },
    { field: 'payDate', header: 'Pay Date' },
    { field: 'status', header: 'Status' },
    { field: 'employeeCount', header: 'Employees' },
    { field: 'totalNetPay', header: 'Total Net Pay' },
  ];

  constructor(private payRunsApi: PayRunsApiService, private router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.payRunsApi
      .getPayRuns({ page: this.page, pageSize: this.pageSize, status: this.selectedStatus || undefined })
      .subscribe({
        next: (result: PaginatedResult<PayRunSummary>) => {
          this.payRuns = result.items.map(pr => ({ ...pr, period: `${pr.periodStart} - ${pr.periodEnd}` }));
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

  goToCreate(): void {
    this.router.navigate(['/payroll/new']);
  }

  viewPayRun(payRun: PayRunSummary): void {
    this.router.navigate(['/payroll', payRun.id]);
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
}
