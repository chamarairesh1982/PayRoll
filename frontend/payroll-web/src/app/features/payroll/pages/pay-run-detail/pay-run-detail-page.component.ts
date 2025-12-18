import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PayRunDetail } from '../../models/pay-run.model';
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
  errorMessage: string | null = null;
  actionComment = '';

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

  viewPaySlip(paySlipId: string): void {
    if (!this.payRun) {
      return;
    }

    this.router.navigate(['/payroll', this.payRun.id, 'payslips', paySlipId]);
  }

  get canRecalculate(): boolean {
    return !!this.payRun && !this.payRun.isLocked && (this.payRun.status === 'Draft' || this.payRun.status === 'Prepared');
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
}
