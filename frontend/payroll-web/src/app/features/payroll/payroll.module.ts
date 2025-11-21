import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { PayRunFormComponent } from './components/pay-run-form/pay-run-form.component';
import { PayRunCreatePageComponent } from './pages/pay-run-create/pay-run-create-page.component';
import { PayRunDetailPageComponent } from './pages/pay-run-detail/pay-run-detail-page.component';
import { PayRunsListPageComponent } from './pages/pay-runs-list/pay-runs-list-page.component';
import { PayslipViewPageComponent } from './pages/payslip-view/payslip-view-page.component';
import { PayrollRoutingModule } from './payroll-routing.module';

@NgModule({
  declarations: [
    PayRunsListPageComponent,
    PayRunCreatePageComponent,
    PayRunDetailPageComponent,
    PayslipViewPageComponent,
    PayRunFormComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, PayrollRoutingModule],
})
export class PayrollModule {}
