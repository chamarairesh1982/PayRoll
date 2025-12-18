import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../../shared/shared.module';
import { AdminRoutingModule } from './admin-routing.module';
import { AuditLogListPageComponent } from './pages/audit-log-list/audit-log-list-page.component';

@NgModule({
  declarations: [AuditLogListPageComponent],
  imports: [SharedModule, FormsModule, AdminRoutingModule],
})
export class AdminModule {}
