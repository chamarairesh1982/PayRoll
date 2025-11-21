import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LeaveCreatePageComponent } from './pages/leave-create/leave-create-page.component';
import { LeaveDetailPageComponent } from './pages/leave-detail/leave-detail-page.component';
import { LeaveEditPageComponent } from './pages/leave-edit/leave-edit-page.component';
import { LeaveListPageComponent } from './pages/leave-list/leave-list-page.component';

const routes: Routes = [
  {
    path: '',
    component: LeaveListPageComponent,
  },
  {
    path: 'new',
    component: LeaveCreatePageComponent,
  },
  {
    path: ':id',
    component: LeaveDetailPageComponent,
  },
  {
    path: ':id/edit',
    component: LeaveEditPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LeaveRoutingModule {}
