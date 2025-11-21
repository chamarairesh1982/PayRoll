import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PayRunDetail, PayRunStatus } from '../../models/pay-run.model';
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
    this.payRunsApi.getPayRun(id).subscribe({
      next: payRun => {
        this.payRun = payRun;
        this.isLoading = false;
      },
      error: err => {
        console.error('Failed to load pay run', err);
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
    if (!this.payRun || this.payRun.isLocked) {
      return;
    }

    if (!confirm(`Change status to ${newStatus}?`)) {
      return;
    }

    this.isChangingStatus = true;
    this.payRunsApi.changeStatus(this.payRun.id, newStatus).subscribe({
      next: () => {
        this.isChangingStatus = false;
        this.loadPayRun();
      },
      error: err => {
        console.error('Failed to change pay run status', err);
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

  get canMarkUnderReview(): boolean {
    return !!this.payRun && !this.payRun.isLocked && (this.payRun.status === 'Draft' || this.payRun.status === 'Calculated');
  }

  get canApprove(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'UnderReview';
  }

  get canPost(): boolean {
    return !!this.payRun && !this.payRun.isLocked && this.payRun.status === 'Approved';
  }
}
