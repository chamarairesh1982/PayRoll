import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RecurringRuleFormPageComponent } from './pages/recurring-rule-form/recurring-rule-form-page.component';
import { RecurringRulesListPageComponent } from './pages/recurring-rules-list/recurring-rules-list-page.component';

const routes: Routes = [
  {
    path: '',
    component: RecurringRulesListPageComponent,
    data: { breadcrumb: 'Recurring Pay Items' },
  },
  {
    path: 'new',
    component: RecurringRuleFormPageComponent,
    data: { breadcrumb: 'New Recurring Pay Item' },
  },
  {
    path: ':id',
    component: RecurringRuleFormPageComponent,
    data: { breadcrumb: 'Recurring Pay Item' },
  },
  {
    path: ':id/edit',
    component: RecurringRuleFormPageComponent,
    data: { breadcrumb: 'Edit Recurring Pay Item' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class RecurringRulesRoutingModule {}
