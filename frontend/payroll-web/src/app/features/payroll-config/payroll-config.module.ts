import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { AllowanceTypeFormComponent } from './components/allowance-type-form/allowance-type-form.component';
import { BankBranchFormComponent } from './components/bank-branch-form/bank-branch-form.component';
import { BankFormComponent } from './components/bank-form/bank-form.component';
import { EpfEtfRuleSetFormComponent } from './components/epf-etf-rule-set-form/epf-etf-rule-set-form.component';
import { TaxRuleSetFormComponent } from './components/tax-rule-set-form/tax-rule-set-form.component';
import { TaxSlabsEditorComponent } from './components/tax-slabs-editor/tax-slabs-editor.component';
import { DeductionTypeFormComponent } from './components/deduction-type-form/deduction-type-form.component';
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
import { PayrollConfigRoutingModule } from './payroll-config-routing.module';
import { OvertimeSettingsFormComponent } from './components/overtime-settings-form/overtime-settings-form.component';

@NgModule({
  declarations: [
    AllowanceTypesListPageComponent,
    AllowanceTypeCreatePageComponent,
    AllowanceTypeEditPageComponent,
    DeductionTypesListPageComponent,
    DeductionTypeCreatePageComponent,
    DeductionTypeEditPageComponent,
    BanksListPageComponent,
    BankCreatePageComponent,
    BankEditPageComponent,
    BankDetailPageComponent,
    BankBranchesListPageComponent,
    BankBranchCreatePageComponent,
    BankBranchEditPageComponent,
    BankBranchDetailPageComponent,
    AllowanceTypeFormComponent,
    DeductionTypeFormComponent,
    BankFormComponent,
    BankBranchFormComponent,
    EpfEtfRulesListPageComponent,
    EpfEtfRuleEditPageComponent,
    EpfEtfRuleSetFormComponent,
    TaxRuleSetsListPageComponent,
    TaxRuleSetEditPageComponent,
    TaxRuleSetFormComponent,
    TaxSlabsEditorComponent,
    OvertimeSettingsPageComponent,
    OvertimeSettingsFormComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, PayrollConfigRoutingModule],
})
export class PayrollConfigModule {}
