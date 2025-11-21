import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { OvertimeFormComponent } from './components/overtime-form/overtime-form.component';
import { OvertimeRoutingModule } from './overtime-routing.module';
import { OvertimeCreatePageComponent } from './pages/overtime-create/overtime-create-page.component';
import { OvertimeDetailPageComponent } from './pages/overtime-detail/overtime-detail-page.component';
import { OvertimeEditPageComponent } from './pages/overtime-edit/overtime-edit-page.component';
import { OvertimeListPageComponent } from './pages/overtime-list/overtime-list-page.component';

@NgModule({
  declarations: [
    OvertimeListPageComponent,
    OvertimeCreatePageComponent,
    OvertimeEditPageComponent,
    OvertimeDetailPageComponent,
    OvertimeFormComponent,
  ],
  imports: [CommonModule, ReactiveFormsModule, SharedModule, OvertimeRoutingModule],
})
export class OvertimeModule {}
