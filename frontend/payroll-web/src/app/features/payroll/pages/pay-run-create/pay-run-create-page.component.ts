import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { PayPeriodType } from '../../models/pay-run.model';
import { PayRunsApiService } from '../../services/pay-runs-api.service';

@Component({
  selector: 'app-pay-run-create-page',
  templateUrl: './pay-run-create-page.component.html',
  styleUrls: ['./pay-run-create-page.component.scss'],
})
export class PayRunCreatePageComponent {
  isSaving = false;

  constructor(private payRunsApi: PayRunsApiService, private router: Router) {}

  handleSubmit(payload: {
    name: string;
    periodType: PayPeriodType;
    periodStart: string;
    periodEnd: string;
    payDate: string;
    includeActiveEmployeesOnly: boolean;
    employeeIds?: string[];
  }): void {
    this.isSaving = true;
    this.payRunsApi.createPayRun(payload).subscribe({
      next: created => {
        this.isSaving = false;
        this.router.navigate(['/payroll', created.id]);
      },
      error: err => {
        console.error('Failed to create pay run', err);
        this.isSaving = false;
      },
    });
  }
}
