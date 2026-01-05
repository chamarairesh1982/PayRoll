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
    data: { breadcrumb: 'Pay Runs' },
  },
  {
    path: 'new',
    component: PayRunCreatePageComponent,
    data: { breadcrumb: 'New Pay Run' },
  },
  {
    path: 'recurring-rules',
    loadChildren: () =>
      import('../recurring-rules/recurring-rules.module').then(m => m.RecurringRulesModule),
    data: { breadcrumb: 'Recurring Pay Items' },
  },
  {
    path: ':payRunId/payslips/:paySlipId',
    component: PayslipViewPageComponent,
    data: { breadcrumb: 'Payslip' },
  },
  {
    path: ':id',
    component: PayRunDetailPageComponent,
    data: { breadcrumb: 'Pay Run Details' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PayrollRoutingModule {}
