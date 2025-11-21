import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { EmployeesRoutingModule } from './employees-routing.module';
import { EmployeesListPageComponent } from './pages/employees-list/employees-list-page.component';
import { EmployeeDetailPageComponent } from './pages/employee-detail/employee-detail-page.component';
import { EmployeeCreatePageComponent } from './pages/employee-create/employee-create-page.component';
import { EmployeeEditPageComponent } from './pages/employee-edit/employee-edit-page.component';
import { EmployeeFormComponent } from './components/employee-form/employee-form.component';

@NgModule({
  declarations: [EmployeesListPageComponent, EmployeeDetailPageComponent, EmployeeCreatePageComponent, EmployeeEditPageComponent, EmployeeFormComponent],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, EmployeesRoutingModule],
})
export class EmployeesModule {}
