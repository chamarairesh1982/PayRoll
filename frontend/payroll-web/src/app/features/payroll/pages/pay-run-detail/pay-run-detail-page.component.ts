import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PayRunDetail, PayRunStatus } from '../../models/pay-run.model';
import { PayRunActionRequest, PayRunsApiService } from '../../services/pay-runs-api.service';

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
  errorMessage: string | null = null;

  constructor(private route: ActivatedRoute, private payRunsApi: PayRunsApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadPayRun();
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

  changeStatus(newStatus: PayRunStatus): void {
    if (!this.payRun) {
      return;
    }

    const action: PayRunActionRequest = { actionedBy: 'web-user' };

    this.isChangingStatus = true;

    let request$: ReturnType<PayRunsApiService['approvePayRun']> | ReturnType<PayRunsApiService['lockPayRun']> | ReturnType<PayRunsApiService['unlockPayRun']>;

    switch (newStatus) {
      case 'Approved':
        request$ = this.payRunsApi.approvePayRun(this.payRun.id, action);
        break;
      case 'Locked':
        request$ = this.payRunsApi.lockPayRun(this.payRun.id, action);
        break;
      default:
        request$ = this.payRunsApi.unlockPayRun(this.payRun.id, action);
        break;
    }

    request$.subscribe({
      next: () => {
        this.isChangingStatus = false;
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to change pay run status', err);
        this.errorMessage = 'Failed to update pay run status.';
        this.isChangingStatus = false;
      },
    });
  }

  viewPaySlip(paySlipId: string): void {
    if (!this.payRun) {
      return;
    }

    this.router.navigate(['/payroll', this.payRun.id, 'payslips', paySlipId]);
  }

  get canRecalculate(): boolean {
    return !!this.payRun && !this.payRun.isLocked && (this.payRun.status === 'Draft' || this.payRun.status === 'Calculated');
  }

  get canApprove(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'Calculated';
  }

  get canLock(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'Approved';
  }

  get canUnlock(): boolean {
    return !!this.payRun && this.payRun.isLocked && this.payRun.status === 'Locked';
  }
}
