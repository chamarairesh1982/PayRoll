import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PayRunCreatePageComponent } from './pages/pay-run-create/pay-run-create-page.component';
import { PayRunDetailPageComponent } from './pages/pay-run-detail/pay-run-detail-page.component';
import { PayRunsListPageComponent } from './pages/pay-runs-list/pay-runs-list-page.component';
import { PayslipViewPageComponent } from './pages/payslip-view/payslip-view-page.component';

const routes: Routes = [
  {
    path: '',
    component: PayRunsListPageComponent,
  },
  {
    path: 'new',
    component: PayRunCreatePageComponent,
  },
  {
    path: ':payRunId/payslips/:paySlipId',
    component: PayslipViewPageComponent,
  },
  {
    path: ':id',
    component: PayRunDetailPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PayrollRoutingModule {}
