import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PayRunListComponent } from './pages/payrun-list/payrun-list.component';
import { PayRunDetailComponent } from './pages/payrun-detail/payrun-detail.component';

const routes: Routes = [
    { path: '', component: PayRunListComponent },
    { path: ':id', component: PayRunDetailComponent }
];

@NgModule({
    declarations: [
        PayRunListComponent,
        PayRunDetailComponent
    ],
    imports: [
        CommonModule,
        RouterModule.forChild(routes),
        FormsModule
    ]
})
export class PayrollsModule { }
