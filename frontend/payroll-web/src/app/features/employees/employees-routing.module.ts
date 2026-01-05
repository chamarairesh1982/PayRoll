import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeesListPageComponent } from './pages/employees-list/employees-list-page.component';
import { EmployeeCreatePageComponent } from './pages/employee-create/employee-create-page.component';
import { EmployeeDetailPageComponent } from './pages/employee-detail/employee-detail-page.component';
import { EmployeeEditPageComponent } from './pages/employee-edit/employee-edit-page.component';

const routes: Routes = [
  { path: '', component: EmployeesListPageComponent, data: { breadcrumb: 'Employees' } },
  { path: 'new', component: EmployeeCreatePageComponent, data: { breadcrumb: 'New Employee' } },
  { path: ':id', component: EmployeeDetailPageComponent, data: { breadcrumb: 'Employee Details' } },
  { path: ':id/edit', component: EmployeeEditPageComponent, data: { breadcrumb: 'Edit Employee' } },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeesRoutingModule {}
