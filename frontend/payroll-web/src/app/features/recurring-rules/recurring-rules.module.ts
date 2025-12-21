import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { RuleFormComponent } from './components/rule-form/rule-form.component';
import { ScopePickerComponent } from './components/scope-picker/scope-picker.component';
import { RecurringRuleFormPageComponent } from './pages/recurring-rule-form/recurring-rule-form-page.component';
import { RecurringRulesListPageComponent } from './pages/recurring-rules-list/recurring-rules-list-page.component';
import { RecurringRulesRoutingModule } from './recurring-rules-routing.module';

@NgModule({
  declarations: [
    RecurringRulesListPageComponent,
    RecurringRuleFormPageComponent,
    RuleFormComponent,
    ScopePickerComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, RecurringRulesRoutingModule],
})
export class RecurringRulesModule {}
