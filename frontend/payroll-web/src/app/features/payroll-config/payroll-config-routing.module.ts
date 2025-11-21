import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AllowanceTypeCreatePageComponent } from './pages/allowance-type-create/allowance-type-create-page.component';
import { AllowanceTypeEditPageComponent } from './pages/allowance-type-edit/allowance-type-edit-page.component';
import { AllowanceTypesListPageComponent } from './pages/allowance-types-list/allowance-types-list-page.component';
import { DeductionTypeCreatePageComponent } from './pages/deduction-type-create/deduction-type-create-page.component';
import { DeductionTypeEditPageComponent } from './pages/deduction-type-edit/deduction-type-edit-page.component';
import { DeductionTypesListPageComponent } from './pages/deduction-types-list/deduction-types-list-page.component';
import { EpfEtfRuleEditPageComponent } from './pages/epf-etf-rule-edit/epf-etf-rule-edit-page.component';
import { EpfEtfRulesListPageComponent } from './pages/epf-etf-rules-list/epf-etf-rules-list-page.component';
import { TaxRuleSetEditPageComponent } from './pages/tax-rule-set-edit/tax-rule-set-edit-page.component';
import { TaxRuleSetsListPageComponent } from './pages/tax-rule-sets-list/tax-rule-sets-list-page.component';

const routes: Routes = [
  {
    path: 'allowances',
    component: AllowanceTypesListPageComponent,
  },
  {
    path: 'allowances/new',
    component: AllowanceTypeCreatePageComponent,
  },
  {
    path: 'allowances/:id/edit',
    component: AllowanceTypeEditPageComponent,
  },
  {
    path: 'deductions',
    component: DeductionTypesListPageComponent,
  },
  {
    path: 'deductions/new',
    component: DeductionTypeCreatePageComponent,
  },
  {
    path: 'deductions/:id/edit',
    component: DeductionTypeEditPageComponent,
  },
  {
    path: 'epf-etf',
    component: EpfEtfRulesListPageComponent,
  },
  {
    path: 'epf-etf/new',
    component: EpfEtfRuleEditPageComponent,
  },
  {
    path: 'epf-etf/:id/edit',
    component: EpfEtfRuleEditPageComponent,
  },
  {
    path: 'tax-rules',
    component: TaxRuleSetsListPageComponent,
  },
  {
    path: 'tax-rules/new',
    component: TaxRuleSetEditPageComponent,
  },
  {
    path: 'tax-rules/:id/edit',
    component: TaxRuleSetEditPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PayrollConfigRoutingModule {}
