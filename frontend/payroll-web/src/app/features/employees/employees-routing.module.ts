import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeesListPageComponent } from './pages/employees-list/employees-list-page.component';
import { EmployeeCreatePageComponent } from './pages/employee-create/employee-create-page.component';
import { EmployeeDetailPageComponent } from './pages/employee-detail/employee-detail-page.component';
import { EmployeeEditPageComponent } from './pages/employee-edit/employee-edit-page.component';

const routes: Routes = [
  { path: '', component: EmployeesListPageComponent },
  { path: 'new', component: EmployeeCreatePageComponent },
  { path: ':id', component: EmployeeDetailPageComponent },
  { path: ':id/edit', component: EmployeeEditPageComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class EmployeesRoutingModule {}
