import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PaySlip } from '../../models/payslip.model';
import { PayRunsApiService } from '../../services/pay-runs-api.service';

@Component({
  selector: 'app-payslip-view-page',
  templateUrl: './payslip-view-page.component.html',
  styleUrls: ['./payslip-view-page.component.scss'],
})
export class PayslipViewPageComponent implements OnInit {
  paySlip?: PaySlip;
  payRunId?: string;
  isLoading = true;

  constructor(private route: ActivatedRoute, private payRunsApi: PayRunsApiService, private router: Router) {}

  ngOnInit(): void {
    this.payRunId = this.route.snapshot.paramMap.get('payRunId') || undefined;
    const paySlipId = this.route.snapshot.paramMap.get('paySlipId');

    if (this.payRunId && paySlipId) {
      this.loadPaySlip(this.payRunId, paySlipId);
    } else {
      this.isLoading = false;
    }
  }

  loadPaySlip(payRunId: string, paySlipId: string): void {
    this.isLoading = true;
    this.payRunsApi.getPaySlip(payRunId, paySlipId).subscribe({
      next: paySlip => {
        this.paySlip = paySlip;
        this.isLoading = false;
      },
      error: err => {
        console.error('Failed to load payslip', err);
        this.isLoading = false;
      },
    });
  }

  goBack(): void {
    if (this.payRunId) {
      this.router.navigate(['/payroll', this.payRunId]);
    }
  }
}
