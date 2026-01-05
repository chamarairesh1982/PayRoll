import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { LeaveRequestFormComponent } from './components/leave-request-form/leave-request-form.component';
import { LeaveRoutingModule } from './leave-routing.module';
import { LeaveCreatePageComponent } from './pages/leave-create/leave-create-page.component';
import { LeaveDetailPageComponent } from './pages/leave-detail/leave-detail-page.component';
import { LeaveEditPageComponent } from './pages/leave-edit/leave-edit-page.component';
import { LeaveListPageComponent } from './pages/leave-list/leave-list-page.component';

@NgModule({
  declarations: [
    LeaveListPageComponent,
    LeaveCreatePageComponent,
    LeaveEditPageComponent,
    LeaveDetailPageComponent,
    LeaveRequestFormComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, LeaveRoutingModule],
})
export class LeaveModule {}
