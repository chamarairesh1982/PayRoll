import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EmployeeListComponent } from './pages/employee-list/employee-list.component';
import { EmployeeFormComponent } from './pages/employee-form/employee-form.component';

const routes: Routes = [
    { path: '', component: EmployeeListComponent },
    { path: 'new', component: EmployeeFormComponent },
    { path: ':id', component: EmployeeFormComponent }
];

@NgModule({
    declarations: [
        EmployeeListComponent,
        EmployeeFormComponent
    ],
    imports: [
        CommonModule,
        RouterModule.forChild(routes),
        FormsModule,
        ReactiveFormsModule
    ]
})
export class EmployeesModule { }
