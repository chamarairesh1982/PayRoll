import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AllowanceTypeCreatePageComponent } from './pages/allowance-type-create/allowance-type-create-page.component';
import { AllowanceTypeEditPageComponent } from './pages/allowance-type-edit/allowance-type-edit-page.component';
import { AllowanceTypesListPageComponent } from './pages/allowance-types-list/allowance-types-list-page.component';
import { DeductionTypeCreatePageComponent } from './pages/deduction-type-create/deduction-type-create-page.component';
import { DeductionTypeEditPageComponent } from './pages/deduction-type-edit/deduction-type-edit-page.component';
import { DeductionTypesListPageComponent } from './pages/deduction-types-list/deduction-types-list-page.component';

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
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PayrollConfigRoutingModule {}
