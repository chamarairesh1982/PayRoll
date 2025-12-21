import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RecurringRuleFormPageComponent } from './pages/recurring-rule-form/recurring-rule-form-page.component';
import { RecurringRulesListPageComponent } from './pages/recurring-rules-list/recurring-rules-list-page.component';

const routes: Routes = [
  {
    path: '',
    component: RecurringRulesListPageComponent,
  },
  {
    path: 'new',
    component: RecurringRuleFormPageComponent,
  },
  {
    path: ':id',
    component: RecurringRuleFormPageComponent,
  },
  {
    path: ':id/edit',
    component: RecurringRuleFormPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class RecurringRulesRoutingModule {}
