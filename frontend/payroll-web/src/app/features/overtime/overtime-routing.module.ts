import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OvertimeCreatePageComponent } from './pages/overtime-create/overtime-create-page.component';
import { OvertimeDetailPageComponent } from './pages/overtime-detail/overtime-detail-page.component';
import { OvertimeEditPageComponent } from './pages/overtime-edit/overtime-edit-page.component';
import { OvertimeListPageComponent } from './pages/overtime-list/overtime-list-page.component';

const routes: Routes = [
  {
    path: '',
    component: OvertimeListPageComponent,
  },
  {
    path: 'new',
    component: OvertimeCreatePageComponent,
  },
  {
    path: ':id',
    component: OvertimeDetailPageComponent,
  },
  {
    path: ':id/edit',
    component: OvertimeEditPageComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class OvertimeRoutingModule {}
