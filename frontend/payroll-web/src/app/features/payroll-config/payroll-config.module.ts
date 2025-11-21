import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { AllowanceTypeFormComponent } from './components/allowance-type-form/allowance-type-form.component';
import { DeductionTypeFormComponent } from './components/deduction-type-form/deduction-type-form.component';
import { AllowanceTypeCreatePageComponent } from './pages/allowance-type-create/allowance-type-create-page.component';
import { AllowanceTypeEditPageComponent } from './pages/allowance-type-edit/allowance-type-edit-page.component';
import { AllowanceTypesListPageComponent } from './pages/allowance-types-list/allowance-types-list-page.component';
import { DeductionTypeCreatePageComponent } from './pages/deduction-type-create/deduction-type-create-page.component';
import { DeductionTypeEditPageComponent } from './pages/deduction-type-edit/deduction-type-edit-page.component';
import { DeductionTypesListPageComponent } from './pages/deduction-types-list/deduction-types-list-page.component';
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
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, PayrollConfigRoutingModule],
})
export class PayrollConfigModule {}
