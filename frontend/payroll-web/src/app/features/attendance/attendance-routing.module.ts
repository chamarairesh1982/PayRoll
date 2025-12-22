import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AttendanceListPageComponent } from './pages/attendance-list/attendance-list-page.component';
import { AttendanceEditPageComponent } from './pages/attendance-edit/attendance-edit-page.component';

const routes: Routes = [
  {
    path: '',
    component: AttendanceListPageComponent,
    data: { breadcrumb: 'Attendance' },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AttendanceRoutingModule {}
