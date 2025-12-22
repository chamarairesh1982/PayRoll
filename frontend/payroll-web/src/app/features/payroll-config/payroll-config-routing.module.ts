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
    data: { breadcrumb: 'Allowances' },
  },
  {
    path: 'allowances/new',
    component: AllowanceTypeCreatePageComponent,
    data: { breadcrumb: 'New Allowance' },
  },
  {
    path: 'allowances/:id/edit',
    component: AllowanceTypeEditPageComponent,
    data: { breadcrumb: 'Edit Allowance' },
  },
  {
    path: 'banks',
    component: BanksListPageComponent,
    data: { breadcrumb: 'Banks' },
  },
  {
    path: 'banks/new',
    component: BankCreatePageComponent,
    data: { breadcrumb: 'New Bank' },
  },
  {
    path: 'banks/:id',
    component: BankDetailPageComponent,
    data: { breadcrumb: 'Bank Details' },
  },
  {
    path: 'banks/:id/edit',
    component: BankEditPageComponent,
    data: { breadcrumb: 'Edit Bank' },
  },
  {
    path: 'bank-branches',
    component: BankBranchesListPageComponent,
    data: { breadcrumb: 'Bank Branches' },
  },
  {
    path: 'bank-branches/new',
    component: BankBranchCreatePageComponent,
    data: { breadcrumb: 'New Bank Branch' },
  },
  {
    path: 'bank-branches/:id',
    component: BankBranchDetailPageComponent,
    data: { breadcrumb: 'Bank Branch Details' },
  },
  {
    path: 'bank-branches/:id/edit',
    component: BankBranchEditPageComponent,
    data: { breadcrumb: 'Edit Bank Branch' },
  },
  {
    path: 'deductions',
    component: DeductionTypesListPageComponent,
    data: { breadcrumb: 'Deductions' },
  },
  {
    path: 'deductions/new',
    component: DeductionTypeCreatePageComponent,
    data: { breadcrumb: 'New Deduction' },
  },
  {
    path: 'deductions/:id/edit',
    component: DeductionTypeEditPageComponent,
    data: { breadcrumb: 'Edit Deduction' },
  },
  {
    path: 'epf-etf',
    component: EpfEtfRulesListPageComponent,
    data: { breadcrumb: 'EPF/ETF Rules' },
  },
  {
    path: 'epf-etf/new',
    component: EpfEtfRuleEditPageComponent,
    data: { breadcrumb: 'New EPF/ETF Rule' },
  },
  {
    path: 'epf-etf/:id/edit',
    component: EpfEtfRuleEditPageComponent,
    data: { breadcrumb: 'Edit EPF/ETF Rule' },
  },
  {
    path: 'tax-rules',
    component: TaxRuleSetsListPageComponent,
    data: { breadcrumb: 'Tax Rules' },
  },
  {
    path: 'overtime',
    component: OvertimeSettingsPageComponent,
    data: { breadcrumb: 'Overtime Rules' },
  },
  {
    path: 'tax-rules/new',
    component: TaxRuleSetEditPageComponent,
    data: { breadcrumb: 'New Tax Rule' },
  },
  {
    path: 'tax-rules/:id/edit',
    component: TaxRuleSetEditPageComponent,
    data: { breadcrumb: 'Edit Tax Rule' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PayrollConfigRoutingModule {}
