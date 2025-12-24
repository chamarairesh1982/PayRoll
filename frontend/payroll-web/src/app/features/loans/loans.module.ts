import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '../../shared/shared.module';
import { LoansListPageComponent } from './pages/loans-list/loans-list-page.component';
import { LoanFormComponent } from './pages/loan-form/loan-form.component';

const routes: Routes = [
    { path: '', component: LoansListPageComponent },
    { path: 'new', component: LoanFormComponent },
    { path: ':id', component: LoanFormComponent },
];

@NgModule({
    declarations: [
        LoansListPageComponent,
        LoanFormComponent,
    ],
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        SharedModule,
        RouterModule.forChild(routes),
    ],
})
export class LoansModule { }
