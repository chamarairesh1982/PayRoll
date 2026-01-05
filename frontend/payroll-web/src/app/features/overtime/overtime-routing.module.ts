import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OvertimeCreatePageComponent } from './pages/overtime-create/overtime-create-page.component';
import { OvertimeApprovalsPageComponent } from './pages/overtime-approvals/overtime-approvals-page.component';
import { OvertimeDetailPageComponent } from './pages/overtime-detail/overtime-detail-page.component';
import { OvertimeEditPageComponent } from './pages/overtime-edit/overtime-edit-page.component';
import { OvertimeListPageComponent } from './pages/overtime-list/overtime-list-page.component';

const routes: Routes = [
  {
    path: '',
    component: OvertimeListPageComponent,
    data: { breadcrumb: 'Overtime' },
  },
  {
    path: 'new',
    component: OvertimeCreatePageComponent,
    data: { breadcrumb: 'New Overtime' },
  },
  {
    path: 'approvals',
    component: OvertimeApprovalsPageComponent,
    data: { breadcrumb: 'Overtime Approvals' },
  },
  {
    path: ':id',
    component: OvertimeDetailPageComponent,
    data: { breadcrumb: 'Overtime Details' },
  },
  {
    path: ':id/edit',
    component: OvertimeEditPageComponent,
    data: { breadcrumb: 'Edit Overtime' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OvertimeRoutingModule {}
