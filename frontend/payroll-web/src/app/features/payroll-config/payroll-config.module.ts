import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { AllowanceTypeFormComponent } from './components/allowance-type-form/allowance-type-form.component';
import { EpfEtfRuleSetFormComponent } from './components/epf-etf-rule-set-form/epf-etf-rule-set-form.component';
import { TaxRuleSetFormComponent } from './components/tax-rule-set-form/tax-rule-set-form.component';
import { TaxSlabsEditorComponent } from './components/tax-slabs-editor/tax-slabs-editor.component';
import { DeductionTypeFormComponent } from './components/deduction-type-form/deduction-type-form.component';
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
import { PayrollConfigRoutingModule } from './payroll-config-routing.module';

@NgModule({
  declarations: [
    AllowanceTypesListPageComponent,
    AllowanceTypeCreatePageComponent,
    AllowanceTypeEditPageComponent,
    DeductionTypesListPageComponent,
    DeductionTypeCreatePageComponent,
    DeductionTypeEditPageComponent,
    AllowanceTypeFormComponent,
    DeductionTypeFormComponent,
    EpfEtfRulesListPageComponent,
    EpfEtfRuleEditPageComponent,
    EpfEtfRuleSetFormComponent,
    TaxRuleSetsListPageComponent,
    TaxRuleSetEditPageComponent,
    TaxRuleSetFormComponent,
    TaxSlabsEditorComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, PayrollConfigRoutingModule],
})
export class PayrollConfigModule {}
