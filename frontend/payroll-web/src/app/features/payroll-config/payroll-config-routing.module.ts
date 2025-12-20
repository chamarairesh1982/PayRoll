import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AllowanceTypeCreatePageComponent } from './pages/allowance-type-create/allowance-type-create-page.component';
import { AllowanceTypeEditPageComponent } from './pages/allowance-type-edit/allowance-type-edit-page.component';
import { AllowanceTypesListPageComponent } from './pages/allowance-types-list/allowance-types-list-page.component';
import { BankBranchCreatePageComponent } from './pages/bank-branch-create/bank-branch-create-page.component';
import { BankBranchDetailPageComponent } from './pages/bank-branch-detail/bank-branch-detail-page.component';
import { BankBranchEditPageComponent } from './pages/bank-branch-edit/bank-branch-edit-page.component';
import { BankBranchesListPageComponent } from './pages/bank-branches-list/bank-branches-list-page.component';
import { BankCreatePageComponent } from './pages/bank-create/bank-create-page.component';
import { BankDetailPageComponent } from './pages/bank-detail/bank-detail-page.component';
import { BankEditPageComponent } from './pages/bank-edit/bank-edit-page.component';
import { BanksListPageComponent } from './pages/banks-list/banks-list-page.component';
import { DeductionTypeCreatePageComponent } from './pages/deduction-type-create/deduction-type-create-page.component';
import { DeductionTypeEditPageComponent } from './pages/deduction-type-edit/deduction-type-edit-page.component';
import { DeductionTypesListPageComponent } from './pages/deduction-types-list/deduction-types-list-page.component';
import { EpfEtfRuleEditPageComponent } from './pages/epf-etf-rule-edit/epf-etf-rule-edit-page.component';
import { EpfEtfRulesListPageComponent } from './pages/epf-etf-rules-list/epf-etf-rules-list-page.component';
import { OvertimeSettingsPageComponent } from './pages/overtime-settings/overtime-settings-page.component';
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
    path: 'banks',
    component: BanksListPageComponent,
  },
  {
    path: 'banks/new',
    component: BankCreatePageComponent,
  },
  {
    path: 'banks/:id',
    component: BankDetailPageComponent,
  },
  {
    path: 'banks/:id/edit',
    component: BankEditPageComponent,
  },
  {
    path: 'bank-branches',
    component: BankBranchesListPageComponent,
  },
  {
    path: 'bank-branches/new',
    component: BankBranchCreatePageComponent,
  },
  {
    path: 'bank-branches/:id',
    component: BankBranchDetailPageComponent,
  },
  {
    path: 'bank-branches/:id/edit',
    component: BankBranchEditPageComponent,
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
    path: 'overtime',
    component: OvertimeSettingsPageComponent,
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
