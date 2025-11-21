import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { AttendanceRoutingModule } from './attendance-routing.module';
import { AttendanceListPageComponent } from './pages/attendance-list/attendance-list-page.component';
import { AttendanceEditPageComponent } from './pages/attendance-edit/attendance-edit-page.component';
import { AttendanceFormComponent } from './components/attendance-form/attendance-form.component';

@NgModule({
  declarations: [AttendanceListPageComponent, AttendanceEditPageComponent, AttendanceFormComponent],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, AttendanceRoutingModule],
})
export class AttendanceModule {}
